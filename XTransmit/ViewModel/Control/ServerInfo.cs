using System.ComponentModel;
using XTransmit.Model.Network;
using XTransmit.Model.Server;

namespace XTransmit.ViewModel.Control
{
    public class ServerInfo : INotifyPropertyChanged
    {
        /** Property SS Info --------------------------------
         */
        public string HostIP
        {
            get => vServerProfile.vHostIP;
            set
            {
                vServerProfile.vHostIP = value;
                OnPropertyChanged("HostIP");
            }
        }

        public int Port
        {
            get => vServerProfile.vPort;
            set
            {
                vServerProfile.vPort = value;
                OnPropertyChanged("Port");
            }
        }

        public string Password
        {
            get => vServerProfile.vPassword;
            set
            {
                vServerProfile.vPassword = value;
                OnPropertyChanged("Password");
            }
        }

        public string Encrypt
        {
            get => vServerProfile.vEncrypt;
            set
            {
                vServerProfile.vEncrypt = value;
                OnPropertyChanged("Encrypt");
            }
        }

        public string Remarks
        {
            get => vServerProfile.vRemarks;
            set
            {
                vServerProfile.vRemarks = value;
                OnPropertyChanged("Remarks");
            }
        }

        public string[] Ciphers { get => ServerProfile.Ciphers; }

        /** Property SS Plugin Info ----------------------------
         */
        public bool PluginEnabled
        {
            get => vServerProfile.vPluginEnabled;
            set
            {
                vServerProfile.vPluginEnabled = value;
                OnPropertyChanged("PluginEnabled");
            }
        }

        public string PluginName
        {
            get => vServerProfile.vPluginName;
            set
            {
                vServerProfile.vPluginName = value;
                OnPropertyChanged("PluginName");
            }
        }

        public string PluginOption
        {
            get => vServerProfile.vPluginOption;
            set
            {
                vServerProfile.vPluginOption = value;
                OnPropertyChanged("PluginOption");
            }
        }

        /** Additional info ----------------------------------
         */
        public string FriendlyName
        {
            get => vServerProfile.vFriendlyName;
            set
            {
                vServerProfile.vFriendlyName = value;
                OnPropertyChanged("FriendlyName");
            }
        }

        public int Timeout
        {
            get => vServerProfile.vTimeout;
            set
            {
                vServerProfile.vTimeout = value;
                OnPropertyChanged("Timeout");
            }
        }

        public string TimeCreated => vServerProfile.vTimeCreated;

        public long Ping
        {
            get => vServerProfile.vPing;
            set
            {
                vServerProfile.vPing = value;
                OnPropertyChanged("Ping");
            }
        }
        
        public void UpdateIPInfo(bool focus)
        {
            vServerProfile.FetchIPData(focus);
            vServerProfile.SetFriendNameByIPData();

            OnPropertyChanged("FriendlyName");
        }
        

        public readonly ServerProfile vServerProfile;

        public ServerInfo(ServerProfile serverProfile) => vServerProfile = serverProfile;
        

        /** INotifyPropertyChanged =================================================================================
         */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
