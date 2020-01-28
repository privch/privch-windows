using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using XTransmit.Model.IPAddress;
using XTransmit.Utility;

namespace XTransmit.Model.Server
{
    [Serializable]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public class ServerProfile : INotifyPropertyChanged
    {
        // encrypt method
        [NonSerialized]
        public static readonly string[] Ciphers =
        {
            "rc4-md5",
            "aes-128-gcm", "aes-192-gcm", "aes-256-gcm",
            "aes-128-cfb", "aes-192-cfb", "aes-256-cfb",
            "aes-128-ctr", "aes-192-ctr", "aes-256-ctr",
            "bf-cfb",
            "camellia-128-cfb", "camellia-192-cfb", "camellia-256-cfb",
            "chacha20", "chacha20-ietf", "chacha20-ietf-poly1305",
            "xchacha20-ietf-poly1305",
            "salsa20",
        };

        /** server arguments
         */
        public string HostIP
        {
            get => hostIp;
            set
            {
                hostIp = value;
                OnPropertyChanged(nameof(HostIP));
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

        public string Encrypt
        {
            get => encrypt;
            set
            {
                encrypt = value;
                OnPropertyChanged(nameof(Encrypt));
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
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

        public bool PluginEnabled
        {
            get => pluginEnabled;
            set
            {
                pluginEnabled = value;
                OnPropertyChanged(nameof(PluginEnabled));
            }
        }

        public string PluginName
        {
            get => pluginName;
            set
            {
                pluginName = value;
                OnPropertyChanged(nameof(PluginName));
            }
        }
        public string PluginOption
        {
            get => pluginOption;
            set
            {
                pluginOption = value;
                OnPropertyChanged(nameof(PluginOption));
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

        public string TimeCreated { get; }

        public IPInfo IPData { get; set; }

        /** status
         */
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
        public long Ping
        {
            get => ping;
            set
            {
                ping = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        // values 
        private string hostIp;
        private int hostPort;
        private string encrypt;
        private string password;
        private string remarks;

        private bool pluginEnabled;
        private string pluginName;
        private string pluginOption;

        private string friendlyName;

        private int listenPort;
        private string responseTime;
        private long ping;

        private static readonly string sr_timedout = (string)Application.Current.FindResource("timed_out");
        private static readonly string sr_failed = (string)Application.Current.FindResource("_failed");

        /**<summary>
         * Must be called after the ConfigManager.Global loaded
         * </summary> 
         */
        public ServerProfile()
        {
            HostIP = "";
            HostPort = 0;
            Encrypt = "chacha20-ietf-poly1305";
            Password = "";
            Remarks = "";

            PluginEnabled = false;
            PluginName = "";
            PluginOption = "";

            FriendlyName = "";
            TimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            IPData = null;

            ListenPort = -1;
            ResponseTime = "";
            Ping = 0;
        }

        public ServerProfile Copy()
        {
            return (ServerProfile)TextUtil.CopyBySerializer(this);
        }

        public bool IsServerEqual(ServerProfile serverNew)
        {
            if (serverNew != null)
            {
                return HostIP == serverNew.HostIP && HostPort == serverNew.HostPort;
            }
            else
            {
                return false;
            }
        }

        public void UpdateIPInfo(bool force)
        {
            if (IPData == null || force)
            {
                IPData = IPInfo.Fetch(HostIP);
                SetFriendNameByIPData();
                OnPropertyChanged(nameof(FriendlyName));
            }
        }

        public void UpdateResponseTime()
        {
            CheckResponseTime();
            OnPropertyChanged(nameof(ResponseTime));
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = string.IsNullOrWhiteSpace(Remarks) ? $"{HostIP} - {HostPort}" : Remarks;
        }

        public void SetFriendNameByIPData()
        {
            if (IPData == null)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(IPData.Country))
            {
                stringBuilder.Append(IPData.Country);
            }

            if (!string.IsNullOrWhiteSpace(IPData.Region))
            {
                stringBuilder.Append(" - " + IPData.Region);
            }

            if (!string.IsNullOrWhiteSpace(IPData.City))
            {
                stringBuilder.Append(" - " + IPData.City);
            }

            string friendlyName = stringBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(friendlyName))
            {
                if (friendlyName.StartsWith(" - ", StringComparison.Ordinal))
                {
                    friendlyName = friendlyName.Substring(3);
                }

                FriendlyName = friendlyName;
            }
        }

        // TODO - UA.
        // return seconds
        private void CheckResponseTime()
        {
            if (ListenPort <= 0)
            {
                return;
            }

            // curl process
            Process process = null;
            int timeout = ConfigManager.Global.SSTimeout;
            try
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = CurlManager.CurlExePath,
                        Arguments = $"--silent --connect-timeout {timeout} --proxy \"socks5://127.0.0.1:{ListenPort}\" " + "-w \"%{time_total}\" -o NUL -s \"https://google.com\"",
                        WorkingDirectory = App.PathCurl,
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

        // OO Programming
        public void CheckPingDelay(Ping ping)
        {
            if (ping == null)
            {
                return;
            }

            try
            {
                PingReply reply = ping.Send(HostIP, ConfigManager.Global.PingTimeout);
                Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
            }
            catch (Exception)
            {
                Ping = -1;
            }
        }


        /** INotifyPropertyChanged =========================================
         */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
