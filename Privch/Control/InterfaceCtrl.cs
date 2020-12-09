using System;
using System.Linq;
using System.Windows;

using MaterialDesignThemes.Wpf;

using PrivCh.Model;
using PrivCh.Utility;
using PrivCh.View;
using PrivCh.ViewModel;
using PrivCh.ViewModel.Element;

namespace PrivCh.Control
{
    internal static class InterfaceCtrl
    {
        public static View.Forms.TrayNotify NotifyIcon;

        public static void Initialize()
        {
            NotifyIcon = new View.Forms.TrayNotify();
            ModifyTheme(theme => theme.SetBaseTheme(
                SettingManager.Appearance.IsDarkTheme ? Theme.Dark : Theme.Light));
        }

        public static void Dispose()
        {
            NotifyIcon.Dispose();
        }

        public static void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }

        public static void ShowSetting()
        {
            DialogSetting dialogSetting = Application.Current.Windows.OfType<DialogSetting>().FirstOrDefault();
            if (dialogSetting == null)
            {
                dialogSetting = new DialogSetting();
                dialogSetting.ShowDialog();
            }
            else
            {
                dialogSetting.Activate();
            }
        }

        public static void OpenCLI()
        {
            string arguments = "/k "
                + $"set http_proxy=127.0.0.1:{SettingManager.Configuration.SystemProxyPort} && "
                + $"set https_proxy=127.0.0.1:{SettingManager.Configuration.SystemProxyPort} && "
                + $"set NO_PROXY=localhost,127.0.0.*,10.*,192.168.*";
            SystemUtil.OpenCLI(arguments);
        }

        public static void ShowHomeNotify(string message)
        {
            if (Application.Current.MainWindow is WindowHome windowHome)
            {
                windowHome.SendSnakebarMessage(message);
            }
        }

        public static void UpdateTransmitLock()
        {
            // WindowHome is null on shutdown. NotifyIcon updates status at menu popup
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.UpdateTransmitLock();
            }

            NotifyIcon.UpdateTransmitLock();
        }

        public static void UpdateHomeTransmitStatue()
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.UpdateTransmitStatus();
            }
        }

        public static void AddHomeTask(TaskView task)
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.AddTask(task);
            }
        }

        public static void RemoveHomeTask(TaskView task)
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.RemoveTask(task);
            }
        }

        public static void AddServerByScanQRCode()
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.AddServerByScanQRCode();
            }
        }

        public static void AddServerFromClipboard()
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.AddServerFromClipboard();
            }
        }
    }
}
