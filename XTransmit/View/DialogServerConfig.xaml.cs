using System;
using System.ComponentModel;
using System.Windows;
using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class DialogServerConfig : Window
    {
        public DialogServerConfig(ServerProfile serverProfile, Action<bool> actionComplete)
        {
            InitializeComponent();

            Preference preference = PreferenceManager.Global;
            Left = preference.WindowServerConfig.X;
            Top = preference.WindowServerConfig.Y;

            // set viewmodel
            DataContext = new ServerConfigVModel(serverProfile, actionComplete);
            Closing += Window_Closing;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save window placement
            Preference preference = PreferenceManager.Global;
            preference.WindowServerConfig.X = Left;
            preference.WindowServerConfig.Y = Top;
        }
    }
}
