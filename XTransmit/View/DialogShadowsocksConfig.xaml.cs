using System;
using System.ComponentModel;
using System.Windows;
using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class DialogShadowsocksConfig : Window
    {
        public DialogShadowsocksConfig(Shadowsocks serverProfile, Action<bool> actionComplete)
        {
            InitializeComponent();

            Preference global = PreferenceManager.Global;
            Left = global.WindowServerConfig.X;
            Top = global.WindowServerConfig.Y;

            // set viewmodel
            DataContext = new ServerConfigVModel(serverProfile, actionComplete);
            Closing += Window_Closing;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // save window placement
            Preference global = PreferenceManager.Global;
            global.WindowServerConfig.X = Left;
            global.WindowServerConfig.Y = Top;
        }
    }
}
