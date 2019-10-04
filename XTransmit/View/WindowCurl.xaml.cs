using System.Windows;
using XTransmit.Model;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /** Curl website list
     * Updated: 2019-10-04
     */
    public partial class WindowCurl : Window
    {
        public WindowCurl()
        {
            InitializeComponent();

            Preference preference = App.GlobalPreference;
            Left = preference.WindowCurl.X;
            Top = preference.WindowCurl.Y;
            Width = preference.WindowCurl.W;
            Height = preference.WindowCurl.H;

            DataContext = new CurlVModel();
            Closing += WindowCurl_Closing;
        }

        private void WindowCurl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Is this a good way?
            ((CurlVModel)DataContext).WindowClose();

            // Save window placement
            Preference preference = App.GlobalPreference;
            preference.WindowCurl.X = Left;
            preference.WindowCurl.Y = Top;
            preference.WindowCurl.W = Width;
            preference.WindowCurl.H = Height;
        }
    }
}
