using System;
using System.ComponentModel;
using System.Windows;

using PrivCh.Model;
using PrivCh.Model.Setting;
using PrivCh.ViewModel;

namespace PrivCh.View
{
    public partial class ServerConfigDialog : Window
    {
        public ServerConfigDialog(BaseServer server, Action<bool> actionComplete)
        {
            InitializeComponent();

            Preference preference = SettingManager.Appearance;
            Left = preference.WindowServerConfig.X;
            Top = preference.WindowServerConfig.Y;

            if (server is Model.SS.Shadowsocks)
            {
                Title = (string)Application.Current.FindResource("dialog_server_shadowsocks");
            }
            else if (server is Model.V2Ray.V2RayVMess)
            {
                Title = (string)Application.Current.FindResource("dialog_server_v2ray");
            }
            else
            {
                throw new ArgumentException(System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            // set viewmodel
            DataContext = new ServerConfigVModel(server, actionComplete);
            Closing += Window_Closing;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // save window placement
            Preference preference = SettingManager.Appearance;
            preference.WindowServerConfig.X = Left;
            preference.WindowServerConfig.Y = Top;
        }
    }
}
