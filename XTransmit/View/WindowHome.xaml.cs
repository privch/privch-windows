using System.Threading.Tasks;
using System.Windows;
using XTransmit.Model;
using XTransmit.Utility;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-06
     */
    public partial class WindowHome : Window
    {
        public WindowHome()
        {
            InitializeComponent();

            Preference preference = App.GlobalPreference;
            Left = preference.WindowHome.X;
            Top = preference.WindowHome.Y;
            Width = preference.WindowHome.W;
            Height = preference.WindowHome.H;

            DataContext = new HomeVModel();
            Closing += Window_Closing;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVisible)
            {
                e.Cancel = true;
                Hide(); // set visiblily?
                return;
            }

            /** if there were other proxy servers running they should set system proxy again
             */
            NativeMethods.DisableProxy();
            PrivoxyManager.Stop();
            SSManager.KillRunning(); // server pool

            // Save window placement
            Preference preference = App.GlobalPreference;
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
            Task.Factory.StartNew(() => messageQueue.Enqueue(message));
        }
    }
}
