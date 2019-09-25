using System;
using System.ComponentModel;
using System.Diagnostics;
using XTransmit.Model.Network;

namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-09-26
     */
    public class SiteWorker
    {
        public Action<bool> OnStateUpdated = null;
        public Action<CurlResponse> OnResponse = null;

        private BackgroundWorker bgWork = null;
        private static readonly Random random = new Random();

        public SiteWorker(Action<bool> OnStateUpdated, Action<CurlResponse> OnResponse)
        {
            this.OnStateUpdated = OnStateUpdated;
            this.OnResponse = OnResponse;
        }

        public void StartBgWork(SiteProfile profile)
        {
            // Init background work
            bgWork = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            bgWork.DoWork += BWDoWork;
            bgWork.ProgressChanged += BWProgressChanged;
            bgWork.RunWorkerCompleted += BWCompleted;

            bgWork.RunWorkerAsync(profile);
        }

        public void StopBgWork()
        {
            if (bgWork == null)
                return;

            if (bgWork.IsBusy)
                bgWork.CancelAsync();

            bgWork.DoWork -= BWDoWork;
            bgWork.ProgressChanged -= BWProgressChanged;
            bgWork.RunWorkerCompleted -= BWCompleted;

            bgWork.Dispose();
        }

        private void BWDoWork(object sender, DoWorkEventArgs e)
        {
            SiteProfile profile = (SiteProfile)e.Argument;

            string profile_arguments = profile.GetArguments();
            FakeIP fakeip = FakeIP.From(profile_arguments); // fake ip

            // report begin state
            bgWork.ReportProgress(-1, null);

            for (int i = 1; i <= profile.PlayTimes; i++)
            {
                string arguments = profile_arguments;
                if (fakeip != null)
                {
                    string ipAddress = fakeip.FakeMethod == FakeIP.Method.Pick ?
                        IPAddressManager.GetRandom() : IPAddressManager.GetGenerate();
                    arguments = arguments.Replace(fakeip.Replace, ipAddress);
                }

                // curl process
                Process process = new Process()
                {
                    StartInfo =
                    {
                        FileName = Utility.CurlManager.PathCurlExe,
                        Arguments = arguments,
                        WorkingDirectory = App.PathCurl,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = profile.IsReadResponse,
                    },
                };
                process.Start();

                string response = process.StartInfo.RedirectStandardOutput ?
                    process.StandardOutput.ReadToEnd() :
                    DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss");
                process.WaitForExit();
                process.Close();

                /** 
                 * report progress, states. 
                 * progress is indicated in e.UserState (value: progressFullness)
                 */
                double progressFullness = (double)i / profile.PlayTimes;
                bgWork.ReportProgress(100, new object[] { progressFullness, i, response });

                /** 
                 * sleep. 
                 * check CancellationPending every 100ms 
                 */
                if (profile.DelayMin <= 0)
                    continue;

                int sleep_count_100ms = profile.DelayMax > profile.DelayMin ?
                    random.Next(profile.DelayMin, profile.DelayMax) * 10
                    : profile.DelayMin * 10;

                while (sleep_count_100ms-- > 0)
                {
                    System.Threading.Thread.Sleep(100);

                    if (bgWork.CancellationPending)
                        return;
                }
            }
        }

        private void BWProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > 0)
            {
                // progress update
                object[] state = (object[])e.UserState;
                int index = (int)state[1];
                string response = (string)state[2];

                OnResponse?.Invoke(new CurlResponse(index, response));
            }
            else
            {
                // state: running
                OnStateUpdated?.Invoke(true);
            }
        }

        private void BWCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // state: not running
            OnStateUpdated?.Invoke(false);
        }
    }
}
