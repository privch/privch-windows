using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Windows;

namespace PrivCh.Model
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public abstract class BaseServer : INotifyPropertyChanged
    {
        // deserializer need to set property
        #region Properties(Serializable)
        public string HostAddress
        {
            get => hostAddress;
            set
            {
                hostAddress = value;
                OnPropertyChanged(nameof(HostAddress));
            }
        }

        public int HostPort
        {
            get => hostPort;
            set
            {
                hostPort = value;
                OnPropertyChanged(nameof(HostPort));
            }
        }

        public string Remarks
        {
            get => remarks;
            set
            {
                remarks = value;
                OnPropertyChanged(nameof(Remarks));
            }
        }

        /** preference and info
         */
        public string FriendlyName
        {
            get => friendlyName;
            set
            {
                friendlyName = value;
                OnPropertyChanged(nameof(FriendlyName));
            }
        }

        // update value after edit
        public string Modified
        {
            get => modified;
            set
            {
                modified = value;
                OnPropertyChanged(nameof(Modified));
            }
        }

        public int ListenPort
        {
            get => listenPort;
            set
            {
                listenPort = value;
                OnPropertyChanged(nameof(ListenPort));
            }
        }

        public string ResponseTime
        {
            get => responseTime;
            set
            {
                responseTime = value;
                OnPropertyChanged(nameof(ResponseTime));
            }
        }

        //seconds, less then 0 means timeout or unreachable
        public long PingDelay
        {
            get => pingDelay;
            set
            {
                pingDelay = value;
                OnPropertyChanged(nameof(PingDelay));
            }
        }
        #endregion


        // values 
        private string hostAddress;
        private int hostPort;
        private string remarks;

        private string friendlyName;
        private string modified;

        private int listenPort;
        private string responseTime;
        private long pingDelay;

        // language strings
        private static readonly string sr_not_checked = (string)Application.Current.FindResource("not_checked");
        private static readonly string sr_timedout = (string)Application.Current.FindResource("timed_out");
        private static readonly string sr_failed = (string)Application.Current.FindResource("_failed");


        public BaseServer()
        {
            hostAddress = string.Empty;
            hostPort = -1;
            remarks = string.Empty;

            friendlyName = string.Empty;
            modified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            listenPort = -1;
            responseTime = sr_not_checked;
            pingDelay = 0;
        }

        public string GetId()
        {
            return $"{hostAddress}:{hostPort}";
        }

        public bool IsServerEqual(object server)
        {
            if (server is BaseServer baseServer)
            {
                return HostAddress == baseServer.HostAddress && HostPort == baseServer.HostPort;
            }
            else
            {
                return false;
            }
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = string.IsNullOrWhiteSpace(Remarks) ? $"{HostAddress} - {HostPort}" : Remarks;
        }

        // return seconds
        public async System.Threading.Tasks.Task UpdateResponseTimeAsync()
        {
            if (ListenPort <= 0)
            {
                return;
            }

            DateTime timeBegin = DateTime.Now;
            Uri uri = new Uri("https://google.com");

            HttpClientHandler httpHandler = new HttpClientHandler
            {
                Proxy = new System.Net.WebProxy("127.0.0.1", SettingManager.Configuration.SystemProxyPort),
                UseProxy = true,
            };

            HttpClient httpClient = new HttpClient(httpHandler, true)
            {
                Timeout = TimeSpan.FromMilliseconds(SettingManager.Configuration.TimeoutCheckResponse),
            };

            try
            {
                var response = await httpClient.GetAsync(uri).ConfigureAwait(true);
                response.EnsureSuccessStatusCode();

                TimeSpan time = DateTime.Now - timeBegin;
                ResponseTime = time.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                TimeSpan time = DateTime.Now - timeBegin;
                ResponseTime = time.TotalMilliseconds < SettingManager.Configuration.TimeoutCheckResponse ?
                    sr_failed : sr_timedout;
            }
            finally
            {
                httpClient.Dispose();
                httpHandler.Dispose();
            }
        }

        public void UpdatePingDelay()
        {
            using (Ping pingSender = new Ping())
            {
                try
                {
                    PingReply reply = pingSender.Send(HostAddress, SettingManager.Configuration.TimeoutPing);
                    PingDelay = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                }
                catch
                {
                    PingDelay = -1;
                }
            }
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
