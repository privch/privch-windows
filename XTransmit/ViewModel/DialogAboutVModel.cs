using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using XTransmit.ViewModel.Element;

/**
 * NOTE - Check for update.
 */
namespace XTransmit.ViewModel
{
    class DialogAboutVModel : BaseViewModel
    {
        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public string Name => App.Name;
        public string Version { get; }

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public ItemView[] OpensourceSoftware { get; }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public DialogAboutVModel()
        {
            // Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            try
            {
                Version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch
            {
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            OpensourceSoftware = new ItemView[]
            {
                new ItemView
                {
                    Label = "MaterialDesignInXamlToolkit", Text = "MIT",
                    Uri = "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE"
                },

                new ItemView
                {
                    Label = "LiveCharts", Text = "MIT",
                    Uri = "https://github.com/beto-rodriguez/Live-Charts/blob/master/LICENSE.TXT"
                },

                new ItemView
                {
                    Label = "ZXing.Net", Text = "Apache-2.0",
                    Uri = "https://www.apache.org/licenses/LICENSE-2.0"
                },

                new ItemView
                {
                    Label = "proxyctrl", Text = "Apache-2.0",
                    Uri = "https://www.apache.org/licenses/LICENSE-2.0"
                },

                new ItemView
                {
                    Label = "privoxy", Text = "LICENSE",
                    Uri = "https://www.privoxy.org/user-manual/copyright.html"
                },

                new ItemView
                {
                    Label = "shadowsocks-libev", Text = "LICENSE",
                    Uri = "https://github.com/shadowsocks/shadowsocks-libev/blob/master/LICENSE"
                },

                new ItemView
                {
                    Label = "V2Ray", Text = "MIT",
                    Uri = "https://raw.githubusercontent.com/v2ray/v2ray-core/master/LICENSE"
                },
            };
        }


        /** Commands ======================================================================================================
         */
        public RelayCommand CommandSendEmail => new RelayCommand(SendEmail);
        private void SendEmail(object parameter)
        {
            string app_email = (string)Application.Current.FindResource("app_email");
            Process.Start($@"mailto://{app_email}"); // return null
        }

        public RelayCommand CommandViewLicense => new RelayCommand(ViewLicense);
        private void ViewLicense(object parameter)
        {
            string uri = (string)parameter;
            Process.Start(uri);
        }
    }
}
