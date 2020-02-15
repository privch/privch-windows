using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows;
using XTransmit.Control;

namespace XTransmit.Model
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
        public void UpdateResponseTime()
        {
            if (ListenPort <= 0)
            {
                return;
            }

            // curl process
            Process process = null;
            int timeout = SettingManager.Configuration.SSTimeout;
            try
            {
                // UA is "curl"
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = ProcCurl.CurlExePath,
                        Arguments = $"--silent --connect-timeout {timeout} --proxy \"socks5://127.0.0.1:{ListenPort}\""
                                    + " -w \"%{time_total}\" -o NUL -s \"https://google.com\"",
                        WorkingDirectory = App.DirectoryCurl,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    });

                ResponseTime = process.StandardOutput.ReadToEnd();
                double time = double.Parse(ResponseTime, CultureInfo.InvariantCulture);
                if (time > timeout)
                {
                    ResponseTime = sr_timedout;
                }

                process.WaitForExit();
            }
            catch
            {
                ResponseTime = sr_failed;
            }
            finally
            {
                process?.Dispose();
            }
        }

        public void UpdatePingDelay()
        {
            using (Ping pingSender = new Ping())
            {
                try
                {
                    PingReply reply = pingSender.Send(HostAddress, SettingManager.Configuration.PingTimeout);
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
