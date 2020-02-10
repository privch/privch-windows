using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using XTransmit.Model.UserAgent;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    class UserAgentVModel : BaseViewModel
    {
        public ObservableCollection<UAProfile> UserAgentListOC { get; private set; }

        [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>")]
        public string Search
        {
            get { return searchValue; }
            set
            {
                searchValue = value;
                if (string.IsNullOrWhiteSpace(searchValue))
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
        private string searchValue;

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
                string sr_yes = (string)Application.Current.FindResource("_yes");
                string sr_no = (string)Application.Current.FindResource("_no");

                bool save = false;
                Dictionary<string, Action> actions = new Dictionary<string, Action>
                {
                    {
                        sr_yes,
                        () => { save = true; }
                    },

                    {
                        sr_no,
                        () => { save = false; }
                    },
                };
                View.DialogAction dialog = new View.DialogAction(title, ask_save, actions);
                dialog.ShowDialog();

                if (save)
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
