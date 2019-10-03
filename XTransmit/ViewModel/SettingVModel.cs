namespace XTransmit.ViewModel
{
    /** 
     * TODO - Add customize option.
     * Updated: 2019-08-06
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
                OnPropertyChanged("IPInfoConnTimeout");
            }
        }

        public int ResponseConnTimeout
        {
            get { return App.GlobalConfig.ResponseConnTimeout; }
            set
            {
                App.GlobalConfig.ResponseConnTimeout = value;
                OnPropertyChanged("ResponseConnTimeout");
            }
        }

        public int PingTimeout
        {
            get { return App.GlobalConfig.PingTimeout; }
            set
            {
                App.GlobalConfig.PingTimeout = value;
                OnPropertyChanged("PingTimeout");
            }
        }

        public SettingVModel()
        {
        }
    }
}
