using System;
using System.Collections.Generic;
using System.Linq;
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

            DataContext = new CurlContentVModel();
            Closing += WindowCurl_Closing;
        }

        private void WindowCurl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // confirm close, if there are tasks running
            bool confirm = false;
            IEnumerable<WindowCurlPlay> curlPlayWindows = Application.Current.Windows.OfType<WindowCurlPlay>();

            foreach (WindowCurlPlay window in curlPlayWindows)
            {
                CurlPlayVModel viewModel = (CurlPlayVModel)window.DataContext;
                if (!confirm && !viewModel.IsNotRunning)
                {
                    string sr_yes = (string)Application.Current.FindResource("_yes");
                    string sr_cancel = (string)Application.Current.FindResource("_cancel");
                    string title = (string)Application.Current.FindResource("curl_title");
                    string message = (string)Application.Current.FindResource("curl_close_play_running");
                    Dictionary<string, Action> actions = new Dictionary<string, Action>()
                    {
                        {
                            sr_yes,
                            ()=> { confirm = true; }
                        },

                        {
                            sr_cancel,
                            null
                        },
                    };

                    new DialogAction(title, message, actions).ShowDialog();
                    if (!confirm)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                window.Close();
            }

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
