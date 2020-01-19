namespace XTransmit.ViewModel
{
    /** 
     * TODO - Add customize option.
     */
    class SettingVModel : BaseViewModel
    {
        public int SSTimeouts
        {
            get => App.GlobalConfig.SSTimeout;
            set
            {
                App.GlobalConfig.SSTimeout = value;
                OnPropertyChanged("ConectionTimeouts");
            }
        }

        public int IPInfoConnTimeout
        {
            get => App.GlobalConfig.IPInfoConnTimeout;
            set
            {
                App.GlobalConfig.IPInfoConnTimeout = value;
                OnPropertyChanged(nameof(IPInfoConnTimeout));
            }
        }

        public int ResponseConnTimeout
        {
            get { return App.GlobalConfig.ResponseConnTimeout; }
            set
            {
                App.GlobalConfig.ResponseConnTimeout = value;
                OnPropertyChanged(nameof(ResponseConnTimeout));
            }
        }

        public int PingTimeout
        {
            get { return App.GlobalConfig.PingTimeout; }
            set
            {
                App.GlobalConfig.PingTimeout = value;
                OnPropertyChanged(nameof(PingTimeout));
            }
        }

        public bool IsReplaceOldServer
        {
            get { return App.GlobalConfig.IsReplaceOldServer; }
            set
            {
                App.GlobalConfig.IsReplaceOldServer = value;
                OnPropertyChanged(nameof(IsReplaceOldServer));
            }
        }

        public SettingVModel()
        {
        }
    }
}
