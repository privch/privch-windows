using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using XTransmit.Model.IPAddress;
using XTransmit.Model.Server;
using XTransmit.Model.UserAgent;
using XTransmit.Utility;

namespace XTransmit.Model.Curl
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal class SiteWorker : IDisposable
    {
        private readonly Action<bool> actionStateUpdated;
        private readonly Action<CurlResponse> actionResponse;

        private BackgroundWorker bgWork = null;
        private static readonly Random random = new Random();

        private static readonly string sr_fake_client_error = (string)Application.Current.FindResource("curl_fake_client_error");
        private static readonly string sr_fake_ip_error = (string)Application.Current.FindResource("curl_fake_ip_error");
        private static readonly string sr_fake_ua_error = (string)Application.Current.FindResource("curl_fake_ua_error");
        private static readonly string sr_complete = (string)Application.Current.FindResource("_complete");
        private static readonly string sr_failed = (string)Application.Current.FindResource("_failed");

        public SiteWorker(Action<bool> actionStateUpdated, Action<CurlResponse> actionResponse)
        {
            this.actionStateUpdated = actionStateUpdated;
            this.actionResponse = actionResponse;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopBgWork();
            }
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

        private string PlaySite(string arguments, FakeClient fakeClient, FakeIP fakeip, FakeUA fakeua, bool readReponse)
        {
            // fake client
            if (fakeClient != null)
            {
                ServerProfile server = ServerManager.GerRendom();
                if (server != null)
                {
                    arguments = arguments.Replace(fakeClient.Replace, $"socks5://127.0.0.1:{server.ListenPort}");
                }
                else
                {
                    throw new Exception(sr_fake_client_error);
                }
            }

            // fake ip
            if (fakeip != null)
            {
                string ip;
                if (fakeip.FakeMethod == FakeIP.Method.Pick)
                {
                    ip = IPManager.GetRandom()?.IP;
                }
                else
                {
                    ip = IPManager.GetGenerate();
                }

                if (!string.IsNullOrWhiteSpace(ip))
                {
                    arguments = arguments.Replace(fakeip.Replace, ip);
                }
                else
                {
                    throw new Exception(sr_fake_ip_error);
                }
            }

            // fake ua
            if (fakeua != null)
            {
                string ua = UAManager.GetRandom()?.Value;
                if (!string.IsNullOrWhiteSpace(ua))
                {
                    arguments = arguments.Replace(fakeua.Replace, ua);
                }
                else
                {
                    throw new Exception(sr_fake_ua_error);
                }
            }

            // curl process
            Process process = null;
            string response;
            try
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = CurlManager.CurlExePath,
                        Arguments = arguments,
                        WorkingDirectory = App.PathCurl,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = readReponse,
                    });
                Debug.WriteLine(arguments);
                response = process.StartInfo.RedirectStandardOutput ? process.StandardOutput.ReadToEnd() : sr_complete;
                process.WaitForExit();
            }
            catch
            {
                response = sr_failed;
            }
            finally
            {
                process?.Dispose();
            }

            return response;
        }

        private void BWDoWork(object sender, DoWorkEventArgs ex)
        {
            SiteProfile profile = (SiteProfile)ex.Argument;
            string arguments = profile.GetArguments();

            FakeClient fakeClient = FakeClient.From(arguments); // fake client
            FakeIP fakeip = FakeIP.From(arguments); // fake ip
            FakeUA fakeua = FakeUA.From(arguments); // fake ua

            // less than 0 means begin
            bgWork.ReportProgress(-1, null);

            for (int i = 1; i <= profile.PlayTimes; i++)
            {
                /** Report progress, states. e.UserState is progress value
                 */
                string time = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss", CultureInfo.InvariantCulture);
                try
                {
                    string response = PlaySite(arguments, fakeClient, fakeip, fakeua, profile.IsReadResponse);
                    bgWork.ReportProgress(100, new object[] { i, time, response });
                }
                catch (Exception error)
                {
                    bgWork.ReportProgress(100, new object[] { i, time, error.Message });
                    return;
                }

                /** No Sleep. Check CancellationPending and continue
                 */
                if (profile.DelayMin <= 0)
                {
                    if (bgWork.CancellationPending)
                    {
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }

                /** Sleep. Check CancellationPending every 100ms 
                 */
                int sleep_count_100ms = profile.DelayMax > profile.DelayMin ?
                    random.Next(profile.DelayMin, profile.DelayMax) * 10
                    : profile.DelayMin * 10;

                while (sleep_count_100ms-- > 0)
                {
                    System.Threading.Thread.Sleep(100);

                    if (bgWork.CancellationPending)
                    {
                        return;
                    }
                }
            }
        }

        private void BWProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > 0)
            {
                // progress update
                object[] state = (object[])e.UserState;
                int index = (int)state[0];
                string time = (string)state[1];
                string response = (string)state[2];

                actionResponse?.Invoke(new CurlResponse(index, time, response));
            }
            else
            {
                // state: running
                actionStateUpdated?.Invoke(true);
            }
        }

        private void BWCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // state: not running
            actionStateUpdated?.Invoke(false);
        }
    }
}
