using System.Windows;
using XTransmit.Control;
using XTransmit.Model;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /**
     * Curl website list
     */
    public partial class WindowCurl : Window
    {
        public WindowCurl()
        {
            InitializeComponent();

            Preference preference = PreferenceManager.Global;
            Left = preference.WindowCurl.X;
            Top = preference.WindowCurl.Y;
            Width = preference.WindowCurl.W;
            Height = preference.WindowCurl.H;

            DataContext = new CurlVModel();
            Closing += WindowCurl_Closing;
        }

        private void WindowCurl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Turn off serverpool
            ServerPoolCtrl.StopServerPool();

            // Save window placement
            Preference preference = PreferenceManager.Global;
            preference.WindowCurl.X = Left;
            preference.WindowCurl.Y = Top;
            preference.WindowCurl.W = Width;
            preference.WindowCurl.H = Height;
        }
    }
}
