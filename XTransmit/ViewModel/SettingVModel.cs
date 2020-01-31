using MaterialDesignThemes.Wpf;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using XTransmit.Control;
using XTransmit.Model;
using XTransmit.Utility;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    class SettingVModel : BaseViewModel
    {
        // options
        public int SSTimeouts
        {
            get => ConfigManager.Global.SSTimeout;
            set
            {
                ConfigManager.Global.SSTimeout = value;
                OnPropertyChanged(nameof(SSTimeouts));
            }
        }

        public int IPInfoConnTimeout
        {
            get => ConfigManager.Global.IPInfoConnTimeout;
            set
            {
                ConfigManager.Global.IPInfoConnTimeout = value;
                OnPropertyChanged(nameof(IPInfoConnTimeout));
            }
        }

        public int ResponseConnTimeout
        {
            get { return ConfigManager.Global.ResponseConnTimeout; }
            set
            {
                ConfigManager.Global.ResponseConnTimeout = value;
                OnPropertyChanged(nameof(ResponseConnTimeout));
            }
        }

        public int PingTimeout
        {
            get { return ConfigManager.Global.PingTimeout; }
            set
            {
                ConfigManager.Global.PingTimeout = value;
                OnPropertyChanged(nameof(PingTimeout));
            }
        }

        public bool IsReplaceOldServer
        {
            get { return ConfigManager.Global.IsReplaceOldServer; }
            set
            {
                ConfigManager.Global.IsReplaceOldServer = value;
                OnPropertyChanged(nameof(IsReplaceOldServer));
            }
        }

        // autorun
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public bool IsAutorun
        {
            get => ConfigManager.Global.IsAutorun;
            set
            {
                if (value)
                {
                    SystemUtil.CreateUserStartupShortcut();
                }
                else
                {
                    SystemUtil.DeleteUserStartupShortcuts();
                }

                ConfigManager.Global.IsAutorun = value;
                OnPropertyChanged(nameof(IsAutorun));
            }
        }

        // theme
        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public bool IsDarkTheme
        {
            get => PreferenceManager.Global.IsDarkTheme;
            set
            {
                PreferenceManager.Global.IsDarkTheme = value;
                InterfaceCtrl.ModifyTheme(theme => theme.SetBaseTheme(value ? Theme.Dark : Theme.Light));
            }
        }

        // status
        public ItemView[] Status { get; }

        public SettingVModel()
        {
            Status = new ItemView[]
            {
                new ItemView
                {
                    Label = (string)Application.Current.FindResource("dialog_setting_status_proxy_port"),
                    Text=ConfigManager.Global.SystemProxyPort.ToString(CultureInfo.InvariantCulture),
                },

                new ItemView
                {
                    Label = (string)Application.Current.FindResource("dialog_setting_status_ss_port"),
                    Text = ConfigManager.Global.GlobalSocks5Port.ToString(CultureInfo.InvariantCulture),
                }
            };
        }
    }
}
