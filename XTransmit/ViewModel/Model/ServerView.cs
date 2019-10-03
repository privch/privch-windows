using System.ComponentModel;
using XTransmit.Model.Server;

namespace XTransmit.ViewModel.Model
{
    /**
     * Updated: 2019-10-02
     */
    public class ServerView : INotifyPropertyChanged
    {
        public string[] Ciphers => ServerProfile.Ciphers;

        /** SS Server Info --------------------------------
         */
        public string HostIP
        {
            get => vServerProfile.HostIP;
            set
            {
                vServerProfile.HostIP = value;
                OnPropertyChanged("HostIP");
            }
        }

        public int HostPort
        {
            get => vServerProfile.HostPort;
            set
            {
                vServerProfile.HostPort = value;
                OnPropertyChanged("HostPort");
            }
        }

        public string Password
        {
            get => vServerProfile.Password;
            set
            {
                vServerProfile.Password = value;
                OnPropertyChanged("Password");
            }
        }

        public string Encrypt
        {
            get => vServerProfile.Encrypt;
            set
            {
                vServerProfile.Encrypt = value;
                OnPropertyChanged("Encrypt");
            }
        }

        public string Remarks
        {
            get => vServerProfile.Remarks;
            set
            {
                vServerProfile.Remarks = value;
                OnPropertyChanged("Remarks");
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
                OnPropertyChanged("PluginEnabled");
            }
        }

        public string PluginName
        {
            get => vServerProfile.PluginName;
            set
            {
                vServerProfile.PluginName = value;
                OnPropertyChanged("PluginName");
            }
        }

        public string PluginOption
        {
            get => vServerProfile.PluginOption;
            set
            {
                vServerProfile.PluginOption = value;
                OnPropertyChanged("PluginOption");
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
                OnPropertyChanged("FriendlyName");
            }
        }

        public int Timeout
        {
            get => vServerProfile.Timeout;
            set
            {
                vServerProfile.Timeout = value;
                OnPropertyChanged("Timeout");
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
                OnPropertyChanged("Ping");
            }
        }

        public void UpdateIPInfo(bool focus)
        {
            vServerProfile.FetchIPData(focus);
            vServerProfile.SetFriendNameByIPData();

            OnPropertyChanged("FriendlyName");
        }

        public void UpdateResponseTime()
        {
            vServerProfile.FetchResponseTime();
            OnPropertyChanged("ResponseTime");
        }


        public readonly ServerProfile vServerProfile;

        public ServerView(ServerProfile serverProfile) => vServerProfile = serverProfile;


        /** INotifyPropertyChanged =================================================================================
         */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
