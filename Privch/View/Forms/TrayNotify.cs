using System;
using System.Windows;

using PrivCh.Control;
using PrivCh.Model;

namespace PrivCh.View.Forms
{
    public class TrayNotify : IDisposable
    {
        private readonly System.Windows.Forms.NotifyIcon notifyIcon;
        private readonly System.Windows.Forms.MenuItem menuitemEnableTransmit;

        private static readonly string sr_server_not_set = (string)Application.Current.FindResource("home_server_not_set");

        public TrayNotify()
        {
            string enable_transmit = (string)Application.Current.FindResource("tray_enable_transmit");
            string scan_qrcode = (string)Application.Current.FindResource("add_server_qrcode");
            string import_clipboard = (string)Application.Current.FindResource("add_server_clipboard");
            string setting = (string)Application.Current.FindResource("_settings");
            string exit = (string)Application.Current.FindResource("_exit");

            menuitemEnableTransmit = new System.Windows.Forms.MenuItem(enable_transmit, MenuItem_EnableTransmit)
            {
                Checked = SettingManager.Configuration.IsTransmitEnabled
            };

            // init notify icon
            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = SettingManager.Configuration.IsTransmitEnabled ?
                    Properties.Resources.privch_on : Properties.Resources.privch_off,
                Visible = true,
            };
            notifyIcon.Click += NotifyIcon_Click;
            notifyIcon.MouseMove += NotifyIcon_MouseMove;

            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.Popup += ContextMenu_Popup;
            contextMenu.MenuItems.Add(menuitemEnableTransmit);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(scan_qrcode, MenuItem_AddServer_ScanQRCode));
            //contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(import_clipboard, MenuItem_AddServer_Clipboard));
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(setting, MenuItem_Setting));
            contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(exit, MenuItem_Exit));

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

        public void UpdateTransmitLock()
        {
            menuitemEnableTransmit.Enabled = !SettingManager.IsServerPoolEnabled;
        }

        public void ShowMessage(string text, string title = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                title = App.Name;
            }

            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = text;
            notifyIcon.ShowBalloonTip(500);
        }

        public void UpdateIcon()
        {
            // ServerPool status are showing in the server list
            notifyIcon.Icon = SettingManager.Configuration.IsTransmitEnabled ?
                Properties.Resources.privch_on :
                Properties.Resources.privch_off;
        }

        /** NotifyIcon Handlers ==================================================================================
         */
        private void NotifyIcon_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (SettingManager.RemoteServer != null)
            {
                notifyIcon.Text = SettingManager.RemoteServer.FriendlyName;
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
            menuitemEnableTransmit.Checked = SettingManager.Configuration.IsTransmitEnabled;
        }

        private void MenuItem_EnableTransmit(object sender, EventArgs e)
        {
            TransmitCtrl.EnableTransmit(!SettingManager.Configuration.IsTransmitEnabled);
            menuitemEnableTransmit.Checked = SettingManager.Configuration.IsTransmitEnabled;

            // update interface
            InterfaceCtrl.UpdateHomeTransmitStatue();
            InterfaceCtrl.NotifyIcon.UpdateIcon();
        }

        private void MenuItem_AddServer_ScanQRCode(object sender, EventArgs e)
        {
            InterfaceCtrl.AddServerByScanQRCode();
        }

        private void MenuItem_AddServer_Clipboard(object sender, EventArgs e)
        {
            InterfaceCtrl.AddServerFromClipboard();
        }

        private void MenuItem_Setting(object sender, EventArgs e)
        {
            InterfaceCtrl.ShowSetting();
        }

        private void MenuItem_Exit(object sender, EventArgs e)
        {
            App.CloseMainWindow();
        }
    }
}
