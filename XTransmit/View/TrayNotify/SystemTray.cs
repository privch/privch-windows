using System;
using System.Windows;

namespace XTransmit.View.TrayNotify
{
    /**
     * Updated: 2019-08-06
     */
    public class SystemTray
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private readonly System.Windows.Forms.MenuItem menuitemEnableTransmit;

        private static readonly string sr_tray_enable_transmit = (string)Application.Current.FindResource("tray_enable_transmit");
        private static readonly string sr_tray_add_server_scan = (string)Application.Current.FindResource("tray_add_server_scan_qrcode");
        private static readonly string sr_tray_exit = (string)Application.Current.FindResource("_exit");

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

        /** NotifyIcon Handlers ==================================================================================
         */
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            App.ShowMainWindow();
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            menuitemEnableTransmit.Checked = App.GlobalConfig.IsTransmitEnabled;
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
            notifyIcon.Dispose();
            notifyIcon = null;
        }
    }
}
