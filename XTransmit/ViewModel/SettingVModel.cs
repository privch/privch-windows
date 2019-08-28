namespace XTransmit.ViewModel
{
    /** 
     * TODO - Add customize option.
     * Updated: 2019-08-06
     */
    class SettingVModel : BaseViewModel
    {
        public int ConectionTimeouts
        {
            get { return App.GlobalConfig.ConnectionTimeouts; }
            set
            {
                App.GlobalConfig.ConnectionTimeouts = value;
                OnPropertyChanged("ConectionTimeouts");
            }
        }

        public int PingTimeouts
        {
            get { return App.GlobalConfig.PingTimeouts; }
            set
            {
                App.GlobalConfig.PingTimeouts = value;
                OnPropertyChanged("PingTimeouts");
            }
        }

        public SettingVModel()
        {
        }
    }
}
