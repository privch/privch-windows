using System.Collections.Generic;
using XTransmit.Model.IPAddress;
using XTransmit.Model.Server;

namespace XTransmit.ViewModel.Element
{
    public class ServerView : BaseViewModel
    {
        public static List<string> Ciphers { get; } = new List<string>(ServerProfile.Ciphers);

        /** SS Server Info --------------------------------
         */
        public string HostIP
        {
            get => serverProfile.HostIP;
            set
            {
                serverProfile.HostIP = value;
                OnPropertyChanged(nameof(HostIP));
            }
        }

        public int HostPort
        {
            get => serverProfile.HostPort;
            set
            {
                serverProfile.HostPort = value;
                OnPropertyChanged(nameof(HostPort));
            }
        }

        public string Password
        {
            get => serverProfile.Password;
            set
            {
                serverProfile.Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string Encrypt
        {
            get => serverProfile.Encrypt;
            set
            {
                serverProfile.Encrypt = value;
                OnPropertyChanged(nameof(Encrypt));
            }
        }

        public string Remarks
        {
            get => serverProfile.Remarks;
            set
            {
                serverProfile.Remarks = value;
                OnPropertyChanged(nameof(Remarks));
            }
        }

        /** SS Plugin Info ----------------------------
         */
        public bool PluginEnabled
        {
            get => serverProfile.PluginEnabled;
            set
            {
                serverProfile.PluginEnabled = value;
                OnPropertyChanged(nameof(PluginEnabled));
            }
        }

        public string PluginName
        {
            get => serverProfile.PluginName;
            set
            {
                serverProfile.PluginName = value;
                OnPropertyChanged(nameof(PluginName));
            }
        }

        public string PluginOption
        {
            get => serverProfile.PluginOption;
            set
            {
                serverProfile.PluginOption = value;
                OnPropertyChanged(nameof(PluginOption));
            }
        }

        /** Additional info ----------------------------------
         */
        public string FriendlyName
        {
            get => serverProfile.FriendlyName;
            set
            {
                serverProfile.FriendlyName = value;
                OnPropertyChanged(nameof(FriendlyName));
            }
        }

        public string TimeCreated => serverProfile.TimeCreated;

        public IPInfo IPData => serverProfile.IPData;

        public string ResponseTime => serverProfile.ResponseTime;

        public long Ping
        {
            get => serverProfile.Ping;
            set
            {
                serverProfile.Ping = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        public void UpdateIPInfo(bool force)
        {
            serverProfile.FetchIPData(force);
            serverProfile.SetFriendNameByIPData();

            OnPropertyChanged(nameof(FriendlyName));
        }

        public void UpdateResponseTime()
        {
            serverProfile.CheckResponseTime();
            OnPropertyChanged(nameof(ResponseTime));
        }

        // Server profile
        private readonly ServerProfile serverProfile;

        public ServerProfile GetvServerProfile()
        {
            return serverProfile;
        }

        public ServerView(ServerProfile serverProfile)
        {
            this.serverProfile = serverProfile;
        }
    }
}
