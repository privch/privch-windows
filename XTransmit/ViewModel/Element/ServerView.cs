using System.Collections.Generic;
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
            get => vServerProfile.HostIP;
            set
            {
                vServerProfile.HostIP = value;
                OnPropertyChanged(nameof(HostIP));
            }
        }

        public int HostPort
        {
            get => vServerProfile.HostPort;
            set
            {
                vServerProfile.HostPort = value;
                OnPropertyChanged(nameof(HostPort));
            }
        }

        public string Password
        {
            get => vServerProfile.Password;
            set
            {
                vServerProfile.Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string Encrypt
        {
            get => vServerProfile.Encrypt;
            set
            {
                vServerProfile.Encrypt = value;
                OnPropertyChanged(nameof(Encrypt));
            }
        }

        public string Remarks
        {
            get => vServerProfile.Remarks;
            set
            {
                vServerProfile.Remarks = value;
                OnPropertyChanged(nameof(Remarks));
            }
        }

        /** SS Plugin Info ----------------------------
         */
        public bool PluginEnabled
        {
            get => vServerProfile.PluginEnabled;
            set
            {
                vServerProfile.PluginEnabled = value;
                OnPropertyChanged(nameof(PluginEnabled));
            }
        }

        public string PluginName
        {
            get => vServerProfile.PluginName;
            set
            {
                vServerProfile.PluginName = value;
                OnPropertyChanged(nameof(PluginName));
            }
        }

        public string PluginOption
        {
            get => vServerProfile.PluginOption;
            set
            {
                vServerProfile.PluginOption = value;
                OnPropertyChanged(nameof(PluginOption));
            }
        }

        /** Additional info ----------------------------------
         */
        public string FriendlyName
        {
            get => vServerProfile.FriendlyName;
            set
            {
                vServerProfile.FriendlyName = value;
                OnPropertyChanged(nameof(FriendlyName));
            }
        }

        public string TimeCreated => vServerProfile.TimeCreated;

        public string ResponseTime => vServerProfile.ResponseTime;

        public long Ping
        {
            get => vServerProfile.Ping;
            set
            {
                vServerProfile.Ping = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        public void UpdateIPInfo(bool force)
        {
            vServerProfile.FetchIPData(force);
            vServerProfile.SetFriendNameByIPData();

            OnPropertyChanged(nameof(FriendlyName));
        }

        public void UpdateResponseTime()
        {
            vServerProfile.CheckResponseTime();
            OnPropertyChanged(nameof(ResponseTime));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public ServerProfile vServerProfile { get; }

        public ServerView(ServerProfile serverProfile)
        {
            vServerProfile = serverProfile;
        }
    }
}
