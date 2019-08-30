using System;
using System.Diagnostics;
using System.Windows;
using XTransmit.ViewModel.Control;

/**
 * NOTE - Check for update.
 * Updated: 2019-08-28
 */
namespace XTransmit.ViewModel
{
    class AboutVModel : BaseViewModel
    {
        public string Name { get; private set; }
        public string Version { get; private set; }

        public ItemInfo[] OpensourceSoftware => new ItemInfo[]
        {
            new ItemInfo {
                Label = "MaterialDesignInXamlToolkit", Text = "MIT",
                Uri = "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE"},
            new ItemInfo {
                Label = "ZXing.Net", Text = "Apache-2.0",
                Uri = "https://www.apache.org/licenses/LICENSE-2.0"},
            new ItemInfo {
                Label = "LiveCharts", Text = "MIT",
                Uri = "https://github.com/beto-rodriguez/Live-Charts/blob/master/LICENSE.TXT"},
            new ItemInfo {
                Label = "proxyctrl", Text = "Apache-2.0",
                Uri = "https://www.apache.org/licenses/LICENSE-2.0"},
        };

        public AboutVModel()
        {
            Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            try
            {
                Version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (Exception)
            {
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }


        /** Commands ======================================================================================================
         */
        public RelayCommand CommandSendEmail => new RelayCommand(sendEmail);
        private void sendEmail(object parameter)
        {
            string app_email = (string)Application.Current.FindResource("app_email");
            Process.Start($@"mailto://{app_email}");
        }

        public RelayCommand CommandViewLicense => new RelayCommand(viewLicense);
        private void viewLicense(object parameter)
        {
            string uri = (string)parameter;
            Process.Start(uri);
        }
    }
}
