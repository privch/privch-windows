using System.Data;
using System.Windows;
using XTransmit.Model.Network;

namespace XTransmit.ViewModel
{
    /** 
     * TODO - UA search, UA filter display
     * Updated: 2019-08-06
     */
    class UserAgentVModel
    {
        public DataTable UserAgentDataTable { get; private set; }

        public UserAgentVModel()
        {
            UserAgentDataTable = UserAgentManager.DataSetUA.Tables[0];
        }

        public void OnWindowClosing()
        {
            // save user-agent data if it has changes, when this window is closing
            if (UserAgentManager.DataSetUA.HasChanges())
            {
                string title = (string)Application.Current.FindResource("ua_title");
                string ask_save = (string)Application.Current.FindResource("ua_ask_save_changes");

                View.DialogButton dialog = new View.DialogButton(title, ask_save);
                dialog.ShowDialog();

                if (dialog.CancelableResult == true)
                {
                    UserAgentManager.Save(App.FileUserAgentXml);
                }
                else
                {
                    UserAgentManager.DataSetUA.RejectChanges();
                }
            }
        }


        /** Commands --------------------------------------------------------------------------
         */
        public RelayCommand CommandSaveData => new RelayCommand(saveData, canSaveData);// save data
        private bool canSaveData(object obj) => UserAgentManager.DataSetUA.HasChanges();
        private void saveData(object parameter)
        {
            UserAgentManager.Save(App.FileUserAgentXml);
        }
    }
}
