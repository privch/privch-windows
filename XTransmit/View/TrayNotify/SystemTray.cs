using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace XTransmit.View.TrayNotify
{
    public class SystemTray : IDisposable
    {
        private readonly System.Windows.Forms.NotifyIcon notifyIcon;
        private readonly System.Windows.Forms.MenuItem menuitemEnableTransmit;

        private static readonly string sr_app_name = (string)Application.Current.FindResource("app_name");
        private static readonly string sr_server_not_set = (string)Application.Current.FindResource("home_server_not_set");
        private static readonly string sr_tray_enable_transmit = (string)Application.Current.FindResource("tray_enable_transmit");
        private static readonly string sr_tray_add_server_scan = (string)Application.Current.FindResource("tray_add_server_scan_qrcode");
        private static readonly string sr_tray_exit = (string)Application.Current.FindResource("_exit");

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public SystemTray()
        {
            menuitemEnableTransmit = new System.Windows.Forms.MenuItem(sr_tray_enable_transmit, MenuItem_EnableTransmit)
            {
                Checked = App.GlobalConfig.IsTransmitEnabled
            };

            // init notify icon
            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = Properties.Resources.XTransmit,
                Visible = true,
            };
            notifyIcon.Click += NotifyIcon_Click;
            notifyIcon.MouseMove += NotifyIcon_MouseMove;

            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.Popup += ContextMenu_Popup;
            contextMenu.MenuItems.Add(menuitemEnableTransmit);
            contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(
                sr_tray_add_server_scan, MenuItem_AddServer_ScanQRCode));
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(
                sr_tray_exit, MenuItem_Exit));

            notifyIcon.ContextMenu = contextMenu;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                notifyIcon.Dispose();
                menuitemEnableTransmit.Dispose();
            }
        }

        public void ShowMessage(string text, string title = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                title = sr_app_name;
            }

            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = text;
            notifyIcon.ShowBalloonTip(500);
        }

        public void SwitchIcon(bool active)
        {
            notifyIcon.Icon = active ?
                Properties.Resources.XTransmit :
                Properties.Resources.XTransmit_Off;
        }

        /** NotifyIcon Handlers ==================================================================================
         */
        private void NotifyIcon_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (App.GlobalConfig.RemoteServer != null)
            {
                notifyIcon.Text = App.GlobalConfig.RemoteServer.FriendlyName;
            }
            else
            {
                notifyIcon.Text = sr_server_not_set;
            }
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (e is System.Windows.Forms.MouseEventArgs mouseEventArgs && mouseEventArgs.Button == System.Windows.Forms.MouseButtons.Left)
            {
                App.ShowMainWindow();
            }
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            menuitemEnableTransmit.Checked = App.GlobalConfig.IsTransmitEnabled;
            menuitemEnableTransmit.Enabled = !App.GlobalConfig.IsServerPoolEnabled;
        }

        private void MenuItem_EnableTransmit(object sender, EventArgs e)
        {
            App.EnableTransmit(!App.GlobalConfig.IsTransmitEnabled);
            menuitemEnableTransmit.Checked = App.GlobalConfig.IsTransmitEnabled;
        }

        private void MenuItem_AddServer_ScanQRCode(object sender, EventArgs e)
        {
            App.AddServerByScanQRCode();
        }

        private void MenuItem_Exit(object sender, EventArgs e)
        {
            App.CloseMainWindow();
        }
    }
}
