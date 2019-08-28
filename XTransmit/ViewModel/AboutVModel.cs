using System;
using System.Diagnostics;
using System.Windows;

/**
 * TODO - License info stuff
 * NOTE - Check for update.
 * Updated: 2019-08-06
 */

namespace XTransmit.ViewModel
{
    class AboutVModel : BaseViewModel
    {
        public string Name { get; private set; }
        public string Version { get; private set; }


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
    }
}
