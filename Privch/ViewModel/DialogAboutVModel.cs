using System.Diagnostics;
using System.Windows;

using PrivCh.ViewModel.Element;

/**
 * NOTE - Check for update.
 */
namespace PrivCh.ViewModel
{
    class DialogAboutVModel : BaseViewModel
    {
        public string Name => App.Name;
        public string Version { get; }

        public ItemView[] OpensourceSoftware { get; }

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
            string uri = App.UriOpenSourceSoftware;
            Process.Start(uri);
        }
    }
}
