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
            try
            {
                Version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch
            {
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
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
            string uri = App.UriOpenSourceSoftwareHtml; //(string)parameter;
            Process.Start(uri);
        }
    }
}
