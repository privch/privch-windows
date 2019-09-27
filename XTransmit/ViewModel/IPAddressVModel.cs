using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using XTransmit.Model.IPAddress;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    /**TODO - Cancelable Ping
     * Updated: 2019-09-28
     */
    class IPAddressVModel : BaseViewModel
    {
        public ProgressInfo Progress { get; private set; }

        public ObservableCollection<IPProfile> IPListOC { get; private set; }

        private static readonly object lock_sync = new object();
        public IPAddressVModel()
        {
            Progress = new ProgressInfo(0, false);
            IPListOC = new ObservableCollection<IPProfile>(IPManager.IPList);

            //http://msdn.microsoft.com/en-us/library/hh198861.aspx
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(IPListOC, lock_sync);
        }

        public void OnWindowClosing()
        {
            // isPingInProcess is also use to cancel task
            isPingInProcess = false;

            // save ip address data if there are changes
            List<IPProfile> iplist = new List<IPProfile>(IPListOC);
            if (IPManager.HasChangesToFile(iplist))
            {
                string title = (string)Application.Current.FindResource("ip_title");
                string ask_save = (string)Application.Current.FindResource("ip_ask_save_data");

                View.DialogButton dialog = new View.DialogButton(title, ask_save);
                dialog.ShowDialog();

                if (dialog.CancelableResult == true)
                {
                    IPManager.IPList = iplist;
                    IPManager.Save();
                }
                else
                {
                    // TODO - Is ObservableCollection write data to list item?
                    IPManager.Reload();
                }
            }
        }


        /** Commands --------------------------------------------------------------------------
         */
        private volatile bool isPingInProcess = false;

        // save data
        public RelayCommand CommandSaveData => new RelayCommand(saveData);
        private void saveData(object parameter)
        {
            List<IPProfile> iplist = new List<IPProfile>(IPListOC);
            if (IPManager.HasChangesToFile(iplist))
            {
                IPManager.IPList = iplist;
                IPManager.Save();
            }
        }

        // reload data from file
        public RelayCommand CommandReload => new RelayCommand(reloadData);
        private void reloadData(object parameter)
        {
            IPManager.Reload();
            IPListOC = new ObservableCollection<IPProfile>(IPManager.IPList);
            OnPropertyChanged("IPListOC");
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
                HashSet<string> ipList = IPManager.Import(openFileDialog.FileName);
                foreach (string ip in ipList)
                {
                    IPListOC.Add(new IPProfile
                    {
                        IP = ip,
                        Ping = 0,
                        Remarks = null,
                    });
                }
            }
        }

        // ping
        public RelayCommand CommandPingCheck => new RelayCommand(pingCheckAsync, canPingCheck);
        private bool canPingCheck(object parameter) => !isPingInProcess;
        private async void pingCheckAsync(object parameter)
        {
            isPingInProcess = true;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            Progress.IsIndeterminate = true;
            Progress.Value = 50;
            OnPropertyChanged("Progress");

            using (Ping ping = new Ping())
            {
                foreach (IPProfile ipProfile in IPListOC)
                {
                    // isPingInProcess is also use to cancel task
                    if (isPingInProcess == false)
                    {
                        return;
                    }

                    // TODO - Config timeout?
                    PingReply reply = await ping.SendPingAsync(ipProfile.IP, 999);
                    ipProfile.Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                }
            }

            Progress.IsIndeterminate = false;
            Progress.Value = 0;
            OnPropertyChanged("Progress");

            isPingInProcess = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}
