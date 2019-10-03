using System.Collections.Generic;
using System.Windows;
using XTransmit.Model.UserAgent;

namespace XTransmit.ViewModel
{
    /** 
     * Updated: 2019-09-28
     */
    class UserAgentVModel : BaseViewModel
    {
        public List<UAProfile> UserAgentList { get; private set; }

        private string v_search;
        public string Search
        {
            get { return v_search; }
            set
            {
                v_search = value;
                if (string.IsNullOrWhiteSpace(v_search))
                {
                    UserAgentList = UAManager.UAList;
                }
                else
                {
                    UserAgentList = UAManager.UAList.FindAll(ua =>
                    {
                        return ua.Value.ToLower().Contains(value.ToLower());
                    });
                }
                OnPropertyChanged("UserAgentList");
            }
        }

        public UserAgentVModel()
        {
            UserAgentList = UAManager.UAList;
        }

        public void OnWindowClosing()
        {
            // save user-agent data if it has changes, when this window is closing
            if (UAManager.HasChangesToFile())
            {
                string title = (string)Application.Current.FindResource("ua_title");
                string ask_save = (string)Application.Current.FindResource("ua_ask_save_changes");

                View.DialogAction dialog = new View.DialogAction(title, ask_save);
                dialog.ShowDialog();

                if (dialog.CancelableResult == true)
                {
                    UAManager.Save();
                }
                else
                {
                    UAManager.Reload();
                }
            }
        }

        /** Commands --------------------------------------------------------------------------
         */
        // save data
        public RelayCommand CommandSaveData => new RelayCommand(SaveData);
        private void SaveData(object parameter)
        {
            UAManager.Save();
        }

        // reload data from file
        public RelayCommand CommandReload => new RelayCommand(ReloadData);
        private void ReloadData(object parameter)
        {
            UAManager.Reload();
            UserAgentList = UAManager.UAList;
            OnPropertyChanged("UserAgentList");
        }
    }
}
