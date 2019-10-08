using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using XTransmit.Model.UserAgent;

namespace XTransmit.ViewModel
{
    /** 
     * Updated: 2019-10-07
     */
    class UserAgentVModel : BaseViewModel
    {
        public ObservableCollection<UAProfile> UserAgentListOC { get; private set; }

        private string search_value;
        public string Search
        {
            get { return search_value; }
            set
            {
                search_value = value;
                if (string.IsNullOrWhiteSpace(search_value))
                {
                    UserAgentListOC = new ObservableCollection<UAProfile>(UAManager.UAList);
                }
                else
                {
                    List<UAProfile> uaList = UAManager.UAList.FindAll(ua => ua.Value.ToLower().Contains(value.ToLower()));
                    UserAgentListOC = new ObservableCollection<UAProfile>(uaList);
                }
                OnPropertyChanged("UserAgentList");
            }
        }

        public UserAgentVModel()
        {
            UserAgentListOC = new ObservableCollection<UAProfile>(UAManager.UAList);
        }

        public void OnWindowClosing()
        {
            // save user-agent data if it has changes, when this window is closing
            List<UAProfile> uaList = new List<UAProfile>(UserAgentListOC);
            if (UAManager.HasChangesToFile(uaList))
            {
                string title = (string)Application.Current.FindResource("ua_title");
                string ask_save = (string)Application.Current.FindResource("ua_ask_save_changes");

                View.DialogAction dialog = new View.DialogAction(title, ask_save);
                dialog.ShowDialog();

                if (dialog.CancelableResult == true)
                {
                    UAManager.Save(uaList);
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
            List<UAProfile> uaList = new List<UAProfile>(UserAgentListOC);
            if (UAManager.HasChangesToFile(uaList))
            {
                UAManager.Save(uaList);
            }
        }

        // reload data from file
        public RelayCommand CommandReload => new RelayCommand(ReloadData);
        private void ReloadData(object parameter)
        {
            UAManager.Reload();
            UserAgentListOC = new ObservableCollection<UAProfile>(UAManager.UAList);
            OnPropertyChanged("UserAgentList");
        }
    }
}
