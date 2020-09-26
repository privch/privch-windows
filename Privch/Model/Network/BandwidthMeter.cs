using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Threading;

namespace Privch.Model.Network
{
    internal class BandwidthMeter : IDisposable
    {
        private readonly Action<long[]> actionSpeedUpdated;
        private NetworkInterface adapter;
        private BackgroundWorker bgWork;

        private long adapterBytesSent;
        private long adapterBytesReceived;

        public BandwidthMeter(Action<long[]> actionSpeedUpdated)
        {
            this.actionSpeedUpdated = actionSpeedUpdated;
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
                Stop();
            }
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
            {
                return;
            }

            if (bgWork.IsBusy)
            {
                bgWork.CancelAsync();
            }

            bgWork.DoWork -= BWDoWork;
            bgWork.ProgressChanged -= BWProgressChanged;
            bgWork.Dispose();
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
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
                catch
                {
                    continue;
                }
            }
        }

        private void BWProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is long[] values)
            {
                // callback, publish values
                actionSpeedUpdated?.Invoke(values);
            }
        }
    }
}
