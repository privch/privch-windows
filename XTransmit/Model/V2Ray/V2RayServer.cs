using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using XTransmit.Model.IPAddress;
using XTransmit.Model.Server;

namespace XTransmit.Model.V2Ray
{
    [Serializable]
    public class V2RayServer : IServer, INotifyPropertyChanged
    {
        #region Properties(Serializable)
        public string Protocol
        {
            get => protocol;
            set
            {
                protocol = value;
                OnPropertyChanged(nameof(Protocol));
            }
        }

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
            get => ping;
            set
            {
                ping = value;
                OnPropertyChanged(nameof(PingDelay));
            }
        }

        /** data
         */
        public string Json
        {
            get => json;
            set
            {
                json = value;
                OnPropertyChanged(nameof(Json));
            }
        }
        #endregion

        // values 
        private string protocol;
        private string hostAddress;
        private int hostPort;

        private string friendlyName;
        private string modified;

        private int listenPort;
        private string responseTime;
        private long ping;

        private string json;
        private object jsonObject;


        public static V2RayServer Create(string protocol, string json)
        {
            if (string.Equals(protocol, "vmess", StringComparison.OrdinalIgnoreCase))
            {
                VMess.VServer[] servers = VMess.ServerFromJson(json);
                if (servers != null)
                {
                    return new V2RayServer
                    {
                        protocol = "VMess",
                        hostAddress = servers[0].address,
                        hostPort = servers[0].port,

                        friendlyName = $"{servers[0].address} - {servers[0].port}",
                        modified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),

                        listenPort = -1,
                        responseTime = "",
                        ping = 0,

                        json = json,
                        jsonObject = servers,
                    };
                }
            }

            return null;
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = $"{HostAddress} - {HostPort}";
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

        public bool IsServerEqual(object server)
        {
            if (server is V2RayServer v2ray)
            {
                return HostAddress == v2ray.HostAddress && HostPort == v2ray.HostPort;
            }
            else
            {
                return false;
            }
        }

        public void UpdateIPInfo(bool force)
        {
            if (IPInfo == null || force)
            {
                IPInfo = IPInformation.Fetch(HostAddress);
                SetFriendNameByIPInfo();
            }
        }

        public void UpdateResponse()
        {
            //throw new NotImplementedException();
        }

        public void UpdatePing()
        {
            //throw new NotImplementedException();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
