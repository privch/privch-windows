using System.Windows;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    /** 
     * TODO - Add custom options.
     */
    class SettingVModel : BaseViewModel
    {
        public int SSTimeouts
        {
            get => App.GlobalConfig.SSTimeout;
            set
            {
                App.GlobalConfig.SSTimeout = value;
                OnPropertyChanged(nameof(SSTimeouts));
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

        public ItemView[] Status { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        public SettingVModel()
        {
            Status = new ItemView[]
            {
                new ItemView
                {
                    Label = (string)Application.Current.FindResource("dialog_setting_status_proxy_port"),
                    Text=App.GlobalConfig.SystemProxyPort.ToString(),
                },

                new ItemView
                {
                    Label = (string)Application.Current.FindResource("dialog_setting_status_ss_port"),
                    Text = App.GlobalConfig.GlobalSocks5Port.ToString(),
                }
            };
        }
    }
}
