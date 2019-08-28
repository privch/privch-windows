using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading;

namespace XTransmit.Model.Network
{
    /**TODO - Start-Stop test
     * Updated: 2019-08-02
     */
    public class BandwidthMeter
    {
        private readonly Action<long[]> OnSpeedUpdated;
        private NetworkInterface adapter = null;
        private BackgroundWorker bgWork = null;

        private long adapterBytesSent = 0;
        private long adapterBytesReceived = 0;

        public BandwidthMeter(Action<long[]> OnSpeedUpdated)
        {
            this.OnSpeedUpdated = OnSpeedUpdated;
        }

        public void SetAdapter(NetworkInterface adapterNew)
        {
            adapter = adapterNew;

            IPInterfaceStatistics statistic = adapter.GetIPStatistics();
            adapterBytesReceived = statistic.BytesReceived;
            adapterBytesSent = statistic.BytesSent;
        }

        public void Start()
        {
            // Init background work
            bgWork = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true,
            };
            bgWork.DoWork += BWDoWork;
            bgWork.ProgressChanged += BWProgressChanged;
            bgWork.RunWorkerAsync();
        }

        public void Stop()
        {
            if (bgWork == null)
                return;

            if (bgWork.IsBusy)
                bgWork.CancelAsync();

            bgWork.DoWork -= BWDoWork;
            bgWork.ProgressChanged -= BWProgressChanged;
            bgWork.Dispose();
        }

        private void BWDoWork(object sender, DoWorkEventArgs e)
        {
            while (!bgWork.CancellationPending)
            {
                Thread.Sleep(1000);

                try
                {
                    IPInterfaceStatistics statistic = adapter.GetIPStatistics();

                    long[] values = new long[]
                    {
                        statistic.BytesReceived - adapterBytesReceived,
                        statistic.BytesSent - adapterBytesSent,
                    };

                    adapterBytesReceived = statistic.BytesReceived;
                    adapterBytesSent = statistic.BytesSent;
                    bgWork.ReportProgress(0, userState: values);
                }
                catch (Exception) { continue; }
            }
        }

        private void BWProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is long[] values)
            {
                // callback, publish values
                OnSpeedUpdated?.Invoke(values);
            }
        }
    }
}
