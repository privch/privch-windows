using System;
using System.Data;
using System.Net.NetworkInformation;
using System.Windows;
using XTransmit.Model.Network;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    /**
     * TODO - Edit tablename
     * TODO - Task progress list, display multi task progress.
     * TODO Next - separate the data and store, or use list instead.
     * 
     * NOTE. once a table has been removed from the dataset, that change can't be rejected.
     * 
     * Updated: 2019-08-02
     */
    class IPAddressVModel : BaseViewModel
    {
        public ProgressInfo Progress { get; private set; }

        public DataTableCollection IPTables => IPAddressManager.DataSetIP.Tables;
        public DataTable IPDataTable { get; private set; }

        private volatile bool hasRemoveTable = false;

        public IPAddressVModel()
        {
            Progress = new ProgressInfo(0, false);
        }

        public void OnTableSelectionChanged(string tableName)
        {
            if (IPTables.Contains(tableName))
            {
                IPDataTable = IPTables[tableName];
                OnPropertyChanged("IPDataTable");
            }
        }

        public void OnWindowClosing()
        {
            // isPingInProcess is also use to cancel task
            isPingInProcess = false;

            // save ip address data if it has changes, when this window close
            if (IPAddressManager.DataSetIP.HasChanges() || hasRemoveTable)
            {
                string title = (string)Application.Current.FindResource("ip_title");
                string ask_save = (string)Application.Current.FindResource("ip_ask_save_data");

                View.DialogButton dialog = new View.DialogButton(title, ask_save);
                dialog.ShowDialog();

                if (dialog.CancelableResult == true)
                {
                    IPAddressManager.Save(App.FileIPAddressXml);
                    hasRemoveTable = false;
                }
                else
                {
                    IPAddressManager.DataSetIP.RejectChanges();
                    hasRemoveTable = false;
                }
            }
        }


        /** Commands --------------------------------------------------------------------------
         */
        private volatile bool isPingInProcess = false;

        // save data
        public RelayCommand CommandSaveData => new RelayCommand(saveData, canSaveData);
        private bool canSaveData(object obj) => IPAddressManager.DataSetIP.HasChanges() || hasRemoveTable;
        private void saveData(object parameter)
        {
            IPAddressManager.Save(App.FileIPAddressXml);
            hasRemoveTable = false;

            OnPropertyChanged("IPTables");
        }

        // ping
        public RelayCommand CommandPingCheck => new RelayCommand(pingCheckAsync, canPingCheck);
        private bool canPingCheck(object parameter) => !isPingInProcess && IPDataTable != null;
        private async void pingCheckAsync(object parameter)
        {
            isPingInProcess = true;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            Progress.IsIndeterminate = true;
            Progress.Value = 50;
            OnPropertyChanged("Progress");

            Ping ping = new Ping();
            foreach (DataRow row in IPDataTable.Rows)
            {
                // isPingInProcess is also use to cancel task
                if (isPingInProcess == false)
                {
                    return;
                }

                if (row["IP"] is string ip)
                {
                    PingReply reply = await ping.SendPingAsync(ip, 600);
                    if (reply.Status == IPStatus.Success)
                    {
                        row["Ping"] = reply.RoundtripTime;
                    }
                    else
                    {
                        row["Ping"] = -1;
                    }

                    OnPropertyChanged("IPDataTable");
                }
            }

            Progress.IsIndeterminate = false;
            Progress.Value = 0;
            OnPropertyChanged("Progress");

            isPingInProcess = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        // add new data to datatable
        public RelayCommand CommandAddData => new RelayCommand(addData, canAddData);
        private bool canAddData(object parameter) => !isPingInProcess && IPDataTable != null;
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
                System.Collections.Generic.HashSet<string> ipList = IPAddressManager.Import(openFileDialog.FileName);
                foreach (string ip in ipList)
                {
                    IPDataTable.Rows.Add(ip, 0);
                }

                OnPropertyChanged("IPDataTable");
            }
        }

        // new table
        public RelayCommand CommandNewTable => new RelayCommand(newTable, canNewTable);
        private bool canNewTable(object obj) => !isPingInProcess && IPDataTable != null;
        private void newTable(object parameter)
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
                System.Collections.Generic.HashSet<string> ipList = IPAddressManager.Import(openFileDialog.FileName);
                if (ipList.Count > 0)
                {
                    DataTable table = new DataTable(DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss")); //TODO - Input/edit TableName
                    table.Columns.Add("IP", typeof(string));
                    table.Columns.Add("Ping", typeof(long));

                    foreach (string ip in ipList)
                    {
                        table.Rows.Add(ip, 0);
                    }

                    IPTables.Add(table);
                    OnPropertyChanged("IPTables");
                }
            }
        }

        // delete table
        public RelayCommand CommandDeleteTable => new RelayCommand(deleteTable, canDeleteTable);
        private bool canDeleteTable(object parameter) => !isPingInProcess && IPDataTable != null;
        private void deleteTable(object parameter)
        {
            try
            {
                IPTables.Remove(IPDataTable);
                IPDataTable = IPTables.Count > 0 ? IPTables[0] : null;

                // rise datatable change
                hasRemoveTable = true;
                OnPropertyChanged("IPTables");
                OnPropertyChanged("IPDataTable");
            }
            catch (Exception) { }
        }

        // copy tablename to clipboard
        public RelayCommand CommandCopyTableName => new RelayCommand(copyTableName, canCopyTableName);
        private bool canCopyTableName(object parameter) => IPDataTable != null;
        private void copyTableName(object parameter)
        {
            Clipboard.SetText(IPDataTable.TableName);
        }
    }
}
