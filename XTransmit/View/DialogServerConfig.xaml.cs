using System.ComponentModel;
using System.Windows;
using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.ViewModel;
using XTransmit.ViewModel.Model;

namespace XTransmit.View
{
    /** 
     * Updated: 2019-08-06
     */
    public partial class DialogServerConfig : Window
    {
        public DialogServerConfig(ServerProfile serverProfile)
        {
            InitializeComponent();

            Preference preference = App.GlobalPreference;
            Left = preference.WindowServerConfig.X;
            Top = preference.WindowServerConfig.Y;

            // set viewmodel
            DataContext = new ServerConfigVModel(new ServerView(serverProfile));
            Closing += Window_Closing;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save window placement
            Preference preference = App.GlobalPreference;
            preference.WindowServerConfig.X = Left;
            preference.WindowServerConfig.Y = Top;
        }
    }
}
