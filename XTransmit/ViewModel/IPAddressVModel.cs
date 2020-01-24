using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using XTransmit.Model;
using XTransmit.Model.IPAddress;

namespace XTransmit.ViewModel
{
    /**
     * TODO - Confirm that ObservableCollection write data directly to the original list/array items,
     *        and not the items copied form original list/array
     * TODO - Optimize or remove the ping check
     */
    class IPAddressVModel : BaseViewModel
    {
        public bool IsProcessingPing { get; private set; } = false;

        public ObservableCollection<IPProfile> IPListOC { get; private set; }

        private static readonly object lock_sync = new object();

        public IPAddressVModel()
        {
            IPListOC = new ObservableCollection<IPProfile>(IPManager.GetIPArray());

            //msdn.microsoft.com/en-us/library/hh198861.aspx
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(IPListOC, lock_sync);
        }

        public void OnWindowClosing()
        {
            // isPingInProcess is also use to cancel task
            IsProcessingPing = false;

            // save ip address data if there are changes
            IPProfile[] ipArray = new List<IPProfile>(IPListOC).ToArray();
            if (IPManager.HasChangesToFile(ipArray))
            {
                string title = (string)Application.Current.FindResource("ip_title");
                string ask_save = (string)Application.Current.FindResource("ip_ask_save_data");
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
                    IPManager.Save(ipArray);
                }
                else
                {
                    IPManager.Reload();
                }
            }
        }


        /** Commands --------------------------------------------------------------------------
         */
        public void StopPing()
        {
            IsProcessingPing = false;
        }

        // save data
        public RelayCommand CommandSaveData => new RelayCommand(SaveData);
        private void SaveData(object parameter)
        {
            IPProfile[] ipArray = new List<IPProfile>(IPListOC).ToArray();
            if (IPManager.HasChangesToFile(ipArray))
            {
                IPManager.Save(ipArray);
            }
        }

        // reload data from file
        public RelayCommand CommandReload => new RelayCommand(ReloadData);
        private void ReloadData(object parameter)
        {
            IPManager.Reload();
            IPListOC = new ObservableCollection<IPProfile>(IPManager.GetIPArray());
            OnPropertyChanged(nameof(IPListOC));
        }

        // add new data to datatable
        public RelayCommand CommandAddData => new RelayCommand(AddData);
        private void AddData(object parameter)
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
        public RelayCommand CommandPingCheck => new RelayCommand(PingCheckAsync, CanPingCheck);
        private bool CanPingCheck(object parameter) => !IsProcessingPing;
        private async void PingCheckAsync(object parameter)
        {
            IsProcessingPing = true;
            OnPropertyChanged(nameof(IsProcessingPing));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            int timeout = ConfigManager.Global.PingTimeout;
            using (Ping ping = new Ping())
            {
                foreach (IPProfile ipProfile in IPListOC)
                {
                    // isPingInProcess is also use to cancel task
                    if (IsProcessingPing == false)
                    {
                        break;
                    }

                    PingReply reply = await ping.SendPingAsync(ipProfile.IP, timeout).ConfigureAwait(true);
                    ipProfile.Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                }
            }

            IsProcessingPing = false;
            OnPropertyChanged(nameof(IsProcessingPing));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}
