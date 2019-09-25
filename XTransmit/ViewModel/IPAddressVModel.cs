using System.Collections.Generic;
using System.Windows;
using XTransmit.Model.Network;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-09-26
     */
    class IPAddressVModel : BaseViewModel
    {
        public ProgressInfo Progress { get; private set; }

        public List<string> IPList { get; private set; }

        public IPAddressVModel()
        {
            Progress = new ProgressInfo(0, false);
            IPList = IPAddressManager.IPList;
        }

        public void OnWindowClosing()
        {
            // save ip address data if it has changes, when this window close
            if (IPAddressManager.HasChanges())
            {
                string title = (string)Application.Current.FindResource("ip_title");
                string ask_save = (string)Application.Current.FindResource("ip_ask_save_data");

                View.DialogButton dialog = new View.DialogButton(title, ask_save);
                dialog.ShowDialog();

                if (dialog.CancelableResult == true)
                {
                    IPAddressManager.Save();
                }
                else
                {
                    IPAddressManager.Reload();
                }
            }
        }


        /** Commands --------------------------------------------------------------------------
         */
        // save data
        public RelayCommand CommandSaveData => new RelayCommand(saveData);
        private void saveData(object parameter)
        {
            IPAddressManager.Save();
            OnPropertyChanged("IPList");
        }

        // add new data to datatable
        public RelayCommand CommandAddData => new RelayCommand(addData);
        private void addData(object parameter)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = App.PathCurrent,
                DefaultExt = "txt",
                Filter = "Text File|*.txt",
                AddExtension = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                HashSet<string> ipList = IPAddressManager.Import(openFileDialog.FileName);
                foreach (string ip in ipList)
                {
                    IPList.Add(ip);
                }

                OnPropertyChanged("IPList");
            }
        }
    }
}
