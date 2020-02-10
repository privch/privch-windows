using System;
using System.ComponentModel;
using System.Windows;
using XTransmit.Model;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class ServerConfigDialog : Window
    {
        public ServerConfigDialog(BaseServer server, Action<bool> actionComplete)
        {
            InitializeComponent();

            Preference preference = PreferenceManager.Global;
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
            Preference preference = PreferenceManager.Global;
            preference.WindowServerConfig.X = Left;
            preference.WindowServerConfig.Y = Top;
        }
    }
}
