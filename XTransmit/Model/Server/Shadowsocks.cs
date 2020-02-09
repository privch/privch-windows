using System;
using System.Collections.Generic;
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
    public class Shadowsocks : IServer, INotifyPropertyChanged
    {
        #region Public-Static
        // encrypt method
        public static List<string> Ciphers { get; } = new List<string>
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
        #endregion

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

        public IPInformation IPInfo { get; set; }

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
        private string encrypt;
        private string password;
        private string remarks;

        private bool pluginEnabled;
        private string pluginName;
        private string pluginOption;

        private string friendlyName;
        private string modified;

        private int listenPort;
        private string responseTime;
        private long pingDelay;

        private static readonly string sr_timedout = (string)Application.Current.FindResource("timed_out");
        private static readonly string sr_failed = (string)Application.Current.FindResource("_failed");

        /**<summary>
         * Must be called after the ConfigManager.Global loaded
         * </summary> 
         */
        public Shadowsocks()
        {
            hostAddress = "";
            hostPort = 0;
            encrypt = "chacha20-ietf-poly1305";
            password = "";
            remarks = "";

            pluginEnabled = false;
            pluginName = "";
            pluginOption = "";

            friendlyName = "";
            modified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            IPInfo = null;

            listenPort = -1;
            responseTime = "";
            pingDelay = 0;
        }

        public Shadowsocks Copy()
        {
            return (Shadowsocks)TextUtil.CopyBySerializer(this);
        }

        public bool IsServerEqual(object server)
        {
            if (server is Shadowsocks shadowsocks)
            {
                return HostAddress == shadowsocks.HostAddress && HostPort == shadowsocks.HostPort;
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

        public void SetFriendNameByIPInfo()
        {
            if (IPInfo == null)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(IPInfo.Country))
            {
                stringBuilder.Append(IPInfo.Country);
            }

            if (!string.IsNullOrWhiteSpace(IPInfo.Region))
            {
                stringBuilder.Append(" - " + IPInfo.Region);
            }

            if (!string.IsNullOrWhiteSpace(IPInfo.City))
            {
                stringBuilder.Append(" - " + IPInfo.City);
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

        #region IServer
        public string GetID()
        {
            return $"{hostAddress}:{hostPort}";
        }

        public void UpdateIPInfo(bool force)
        {
            if (IPInfo == null || force)
            {
                IPInfo = IPInformation.Fetch(HostAddress);
                SetFriendNameByIPInfo();
            }
        }

        // return seconds
        public void UpdateResponse()
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
                // UA is "curl"
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = CurlManager.CurlExePath,
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

        public void UpdatePing()
        {
            using (Ping pingSender = new Ping())
            {
                try
                {
                    PingReply reply = pingSender.Send(HostAddress, ConfigManager.Global.PingTimeout);
                    PingDelay = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                }
                catch
                {
                    PingDelay = -1;
                }
            }
        }
        #endregion IServer

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
