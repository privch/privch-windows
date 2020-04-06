using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Privch.Control;
using Privch.Model;
using Privch.Model.V2Ray;
using Privch.Utility;
using Privch.View;
using Privch.ViewModel.Element;

namespace Privch.ViewModel
{
    /** NOTE
     * V2Ray have not ip information
     */
    class ContentV2RayVModel : BaseServerVModel
    {
        public ObservableCollection<V2RayVMess> V2RayOC { get; private set; }

        // language
        private static readonly string sr_server_not_found = (string)Application.Current.FindResource("add_server_not_found");
        private static readonly string sr_server_already_exist = (string)Application.Current.FindResource("add_server_already_exist");
        private static readonly string sr_server_x_added = (string)Application.Current.FindResource("add_server_x_added");
        private static readonly string sr_server_x_updated = (string)Application.Current.FindResource("add_server_x_updated");

        public ContentV2RayVModel()
        {
            // load servers and convert to ObservableCollection
            V2RayOC = new ObservableCollection<V2RayVMess>(ServerManager.V2RayList);
            Servers = V2RayOC.Cast<BaseServer>();
        }

        ~ContentV2RayVModel()
        {
            // cancel tasks
            processing_check_ping = false;

            // data changed ?
            SaveServer(null);
        }


        private void AddServer(List<V2RayVMess> serverList, out int added, out int updated)
        {
            added = 0;
            updated = 0;

            foreach (V2RayVMess server in serverList)
            {
                V2RayVMess serverOld = V2RayOC.FirstOrDefault(predicate: x => x.IsServerEqual(server));
                if (serverOld == null)
                {
                    V2RayOC.Add(server);
                    ++added;
                }
                else
                {
                    if (SettingManager.Configuration.IsReplaceOldServer)
                    {
                        int i = V2RayOC.IndexOf(serverOld);
                        V2RayOC[i] = server;
                        ++updated;

                        // keep status
                        if (serverOld.ListenPort > 0)
                        {
                            if (TransmitCtrl.ChangeTransmitServer(server, true))
                            {
                                InterfaceCtrl.UpdateHomeTransmitStatue();
                            }
                        }
                    }
                }
            }
        }

        private void AddServer(V2RayVMess server, out int added, out int updated)
        {
            added = 0;
            updated = 0;

            V2RayVMess serverOld = V2RayOC.FirstOrDefault(predicate: x => x.IsServerEqual(server));
            if (serverOld == null)
            {
                V2RayOC.Add(server);
                ++added;
            }
            else
            {
                if (SettingManager.Configuration.IsReplaceOldServer)
                {
                    int i = V2RayOC.IndexOf(serverOld);
                    V2RayOC[i] = server;
                    ++updated;

                    // keep status
                    if (serverOld.ListenPort > 0)
                    {
                        if (TransmitCtrl.ChangeTransmitServer(server, true))
                        {
                            InterfaceCtrl.UpdateHomeTransmitStatue();
                        }
                    }
                }
            }
        }

        private bool IsServerFree(object parameter)
        {
            if (parameter is V2RayVMess server)
            {
                if (!server.IsServerEqual(SettingManager.RemoteServer))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanEditList(object parameter)
        {
            return !processing_check_ping;
        }

        #region Commands
        // save data
        public RelayCommand CommandSaveServer => new RelayCommand(SaveServer);
        private void SaveServer(object parameter)
        {
            // convert to list and save
            List<V2RayVMess> profiles = new List<V2RayVMess>(V2RayOC);
            ServerManager.Save(profiles);
        }

        // change server
        public RelayCommand CommandChangeServer => new RelayCommand(ChangeServer, IsServerFree);
        private void ChangeServer(object parameter)
        {
            V2RayVMess server = (V2RayVMess)parameter;
            if (TransmitCtrl.ChangeTransmitServer(server))
            {
                InterfaceCtrl.UpdateHomeTransmitStatue();
            }
        }

        // add server by scan qrcode
        public RelayCommand CommandAddServerQRCode => new RelayCommand(AddServerQRCode, CanEditList);
        private void AddServerQRCode(object parameter)
        {
            string ssBase64 = parameter as string;
            if (string.IsNullOrWhiteSpace(ssBase64))
            {
                ZXing.Result result = QRCode.DecodeScreen();
                if (result == null || string.IsNullOrWhiteSpace(result.Text))
                {
                    InterfaceCtrl.ShowHomeNotify(sr_server_not_found);
                    InterfaceCtrl.NotifyIcon.ShowMessage(sr_server_not_found);
                    return;
                }

                ssBase64 = result.Text;
            }

            if (V2RayVMess.FromVMessBase64(ssBase64) is V2RayVMess server)
            {
                AddServer(server, out int added, out int updated);

                string notify;
                if (SettingManager.Configuration.IsReplaceOldServer)
                {
                    notify = added > 0 ? $"{added} {sr_server_x_added}" : $"{updated} {sr_server_x_updated}";
                }
                else
                {
                    notify = added > 0 ? $"{added} {sr_server_x_added}" : sr_server_already_exist;
                }

                if (Application.Current.MainWindow.IsActive)
                {
                    InterfaceCtrl.ShowHomeNotify(notify);
                }
                else
                {
                    InterfaceCtrl.NotifyIcon.ShowMessage(notify);
                }
            }
            else
            {
                if (Application.Current.MainWindow.IsActive)
                {
                    InterfaceCtrl.ShowHomeNotify(sr_server_not_found);
                }
                else
                {
                    InterfaceCtrl.NotifyIcon.ShowMessage(sr_server_not_found);
                }
            }
        }

        // add server by import clipboard
        public RelayCommand CommandAddServerClipboard => new RelayCommand(AddServerClipboard, CanEditList);
        private void AddServerClipboard(object parameter)
        {
            List<V2RayVMess> serverList = V2RayVMess.ImportServers(Clipboard.GetText(TextDataFormat.Text));
            if (serverList.Count > 0)
            {
                AddServer(serverList, out int added, out int updated);

                string notify;
                if (SettingManager.Configuration.IsReplaceOldServer)
                {
                    notify = $"{added} {sr_server_x_added}, {updated} {sr_server_x_updated}";
                }
                else
                {
                    notify = $"{added} {sr_server_x_added}";
                }

                InterfaceCtrl.ShowHomeNotify(notify);
            }
            else
            {
                InterfaceCtrl.ShowHomeNotify(sr_server_not_found);
            }
        }

        // add server by import file
        public RelayCommand CommandAddServerFile => new RelayCommand(AddServerFile, CanEditList);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void AddServerFile(object parameter)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = App.DirectoryApplication,
                DefaultExt = "txt",
                Filter = "Text|*.txt",
                AddExtension = true
            };

            string fileContent = null;
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    fileContent = System.IO.File.ReadAllText(openFileDialog.FileName);
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return;
            }

            List<V2RayVMess> serverList = V2RayVMess.ImportServers(fileContent);
            if (serverList.Count > 0)
            {
                AddServer(serverList, out int added, out int updated);

                string notify;
                if (SettingManager.Configuration.IsReplaceOldServer)
                {
                    notify = $"{added} {sr_server_x_added}, {updated} {sr_server_x_updated}";
                }
                else
                {
                    notify = $"{added} {sr_server_x_added}";
                }

                InterfaceCtrl.ShowHomeNotify(notify);
            }
            else
            {
                InterfaceCtrl.ShowHomeNotify(sr_server_not_found);
            }
        }

        // add server by manual config
        public RelayCommand CommandAddServerCreate => new RelayCommand(AddServerCreate, CanEditList);
        private void AddServerCreate(object parameter)
        {
            V2RayVMess server = new V2RayVMess();

            new ServerConfigDialog(server,
                (bool resultOK) =>
                {
                    if (resultOK)
                    {
                        AddServer(server, out int added, out int updated);

                        string notify;
                        if (SettingManager.Configuration.IsReplaceOldServer)
                        {
                            notify = added > 0 ? $"{added} {sr_server_x_added}" : $"{updated} {sr_server_x_updated}";
                        }
                        else
                        {
                            notify = added > 0 ? $"{added} {sr_server_x_added}" : sr_server_already_exist;
                        }

                        InterfaceCtrl.ShowHomeNotify(notify);
                    }
                }).ShowDialog();
        }

        // edit server
        public RelayCommand CommandEditServer => new RelayCommand(EditServer, IsServerFree);
        private void EditServer(object parameter)
        {
            V2RayVMess serverOld = (V2RayVMess)parameter;
            V2RayVMess serverNew = serverOld.Copy();

            new ServerConfigDialog(serverNew,
                (bool resultOK) =>
                {
                    if (resultOK)
                    {
                        int i = V2RayOC.IndexOf(serverOld);
                        if (i >= 0)
                        {
                            V2RayOC[i] = serverNew;
                        }
                    }
                }).ShowDialog();
        }

        // delete selected server(s)
        public RelayCommand CommandDeleteServers => new RelayCommand(DeleteServers, CanDeleteServer);
        private bool CanDeleteServer(object parameter)
        {
            return (V2RayOC.Count > 0)
                && parameter is System.Collections.IList
                && CanEditList(null);
        }
        private void DeleteServers(object parameter)
        {
            /** https://stackoverflow.com/a/14852516
             */
            System.Collections.IList selection = parameter as System.Collections.IList;
            List<V2RayVMess> servers = selection.Cast<V2RayVMess>().ToList();

            foreach (V2RayVMess server in servers)
            {
                V2RayOC.Remove(server);
            }
        }
        #endregion 
    }
}
