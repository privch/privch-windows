using System.Threading.Tasks;
using System.Windows;
using Privch.Model;
using Privch.Model.Setting;
using Privch.ViewModel;

namespace Privch.View
{
    public partial class WindowHome : Window
    {
        public WindowHome()
        {
            InitializeComponent();

            Preference preference = SettingManager.Appearance;
            Left = preference.WindowHome.X;
            Top = preference.WindowHome.Y;
            Width = preference.WindowHome.W;
            Height = preference.WindowHome.H;

            Visibility = preference.IsWindowHomeVisible ? Visibility.Visible : Visibility.Hidden;

            DataContext = new HomeVModel();
            Closing += Window_Closing;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // hide but not exit
            if (IsVisible)
            {
                e.Cancel = true;
                Hide(); // NOTE - Set visiblily property?
                return;
            }

            // save preference
            HomeVModel viewModel = (HomeVModel)DataContext;
            Preference preference = SettingManager.Appearance;
            preference.HomeContentDisplay = viewModel.GetCurrentContent();

            // window placement
            preference.WindowHome.X = Left;
            preference.WindowHome.Y = Top;
            preference.WindowHome.W = Width;
            preference.WindowHome.H = Height;
        }

        public void SendSnakebarMessage(string message)
        {
            // use the message queue to send a message.
            var messageQueue = xSnackbarNotify.MessageQueue;

            // the message queue can be called from any thread
            Task.Run(() => messageQueue.Enqueue(message));
        }
    }
}
