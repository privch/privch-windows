using MaterialDesignThemes.Wpf;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using Privch.Control;
using Privch.Model;
using Privch.Utility;
using Privch.ViewModel.Element;

namespace Privch.ViewModel
{
    class SettingVModel : BaseViewModel
    {
        // options
        public int SSTimeouts
        {
            get => SettingManager.Configuration.TimeoutShadowsocks;
            set
            {
                SettingManager.Configuration.TimeoutShadowsocks = value;
                OnPropertyChanged(nameof(SSTimeouts));
            }
        }

        public int IPInfoConnTimeout
        {
            get => SettingManager.Configuration.TimeoutUpdateInfo;
            set
            {
                SettingManager.Configuration.TimeoutUpdateInfo = value;
                OnPropertyChanged(nameof(IPInfoConnTimeout));
            }
        }

        public int ResponseConnTimeout
        {
            get { return SettingManager.Configuration.TimeoutCheckResponse; }
            set
            {
                SettingManager.Configuration.TimeoutCheckResponse = value;
                OnPropertyChanged(nameof(ResponseConnTimeout));
            }
        }

        public int PingTimeout
        {
            get { return SettingManager.Configuration.TimeoutPing; }
            set
            {
                SettingManager.Configuration.TimeoutPing = value;
                OnPropertyChanged(nameof(PingTimeout));
            }
        }

        public bool IsReplaceOldServer
        {
            get { return SettingManager.Configuration.IsReplaceOldServer; }
            set
            {
                SettingManager.Configuration.IsReplaceOldServer = value;
                OnPropertyChanged(nameof(IsReplaceOldServer));
            }
        }

        // autorun
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public bool IsAutorun
        {
            get => SettingManager.Configuration.IsAutorun;
            set
            {
                if (value)
                {
                    SystemUtil.CheckOrCreateUserStartupShortcut();
                }
                else
                {
                    SystemUtil.DeleteUserStartupShortcuts();
                }

                SettingManager.Configuration.IsAutorun = value;
                OnPropertyChanged(nameof(IsAutorun));
            }
        }

        // theme
        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public bool IsDarkTheme
        {
            get => SettingManager.Appearance.IsDarkTheme;
            set
            {
                SettingManager.Appearance.IsDarkTheme = value;
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
                    Text=SettingManager.Configuration.SystemProxyPort.ToString(CultureInfo.InvariantCulture),
                },

                new ItemView
                {
                    Label = (string)Application.Current.FindResource("dialog_setting_status_ss_port"),
                    Text = SettingManager.Configuration.LocalSocks5Port.ToString(CultureInfo.InvariantCulture),
                }
            };
        }
    }
}
