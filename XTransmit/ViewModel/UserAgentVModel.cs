using System.Collections.Generic;
using System.Windows;
using XTransmit.Model.UserAgent;

namespace XTransmit.ViewModel
{
    /** 
     * Updated: 2019-09-24
     */
    class UserAgentVModel : BaseViewModel
    {
        public List<UserAgentProfile> UserAgentList { get; private set; }

        private string vSearch;
        public string Search
        {
            get { return vSearch; }
            set
            {
                vSearch = value;
                if (string.IsNullOrWhiteSpace(vSearch))
                {
                    UserAgentList = UserAgentManager.ListUserAgent;
                }
                else
                {
                    UserAgentList = UserAgentManager.ListUserAgent.FindAll(ua =>
                    {
                        return ua.value.ToLower().Contains(value.ToLower());
                    });
                }
                OnPropertyChanged("UserAgentList");
            }
        }

        public UserAgentVModel()
        {
            UserAgentList = UserAgentManager.ListUserAgent;
        }

        // TODO - HasChanges
        public void OnWindowClosing()
        {
            // save user-agent data if it has changes, when this window is closing
            if (UserAgentManager.HasChanges())
            {
                string title = (string)Application.Current.FindResource("ua_title");
                string ask_save = (string)Application.Current.FindResource("ua_ask_save_changes");

                View.DialogButton dialog = new View.DialogButton(title, ask_save);
                dialog.ShowDialog();

                if (dialog.CancelableResult == true)
                {
                    UserAgentManager.Save();
                }
                else
                {
                    UserAgentManager.Reload();
                }
            }
        }

        /** Commands --------------------------------------------------------------------------
         */
        // save data
        public RelayCommand CommandSaveData => new RelayCommand(saveData);
        private void saveData(object parameter)
        {
            UserAgentManager.Save();
        }

        // reload data from file
        public RelayCommand CommandReload => new RelayCommand(reloadData);
        private void reloadData(object parameter)
        {
            UserAgentManager.Reload();
            UserAgentList = UserAgentManager.ListUserAgent;
            OnPropertyChanged("UserAgentList");
        }
    }
}
