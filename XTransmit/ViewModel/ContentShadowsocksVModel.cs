using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XTransmit.Control;
using XTransmit.Model;
using XTransmit.Model.SS;
using XTransmit.Utility;
using XTransmit.View;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    class ContentShadowsocksVModel : BaseServerVModel
    {
        public ObservableCollection<Shadowsocks> ShadowsocksOC { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1822", Justification = "<Pending>")]
        public bool IsServerPoolEnabled
        {
            get => SettingManager.IsServerPoolEnabled;
            set
            {
                if (value)
                {
                    ServerPoolCtrl.StartServerPool();
                }
                else
                {
                    ServerPoolCtrl.StopServerPool();
                }
            }
        }

        // status
        private volatile bool processing_update_info = false;

        // language
        private static readonly string sr_yes = (string)Application.Current.FindResource("_yes");
        private static readonly string sr_cancel = (string)Application.Current.FindResource("_cancel");

        private static readonly string sr_server_not_found = (string)Application.Current.FindResource("add_server_not_found");
        private static readonly string sr_server_already_exist = (string)Application.Current.FindResource("add_server_already_exist");
        private static readonly string sr_server_x_added = (string)Application.Current.FindResource("add_server_x_added");
        private static readonly string sr_server_x_updated = (string)Application.Current.FindResource("add_server_x_updated");

        private static readonly string sr_update_info = (string)Application.Current.FindResource("task_update_info");
        private static readonly string sr_update_info_confirm = (string)Application.Current.FindResource("server_update_info_confirm");

        public ContentShadowsocksVModel()
        {
            // load servers and convert to ObservableCollection
            ShadowsocksOC = new ObservableCollection<Shadowsocks>(ServerManager.ShadowsocksList);
            Servers = ShadowsocksOC.Cast<BaseServer>();
        }

        ~ContentShadowsocksVModel()
        {
            // cancel tasks
            processing_update_info = false;
            processing_check_ping = false;

            // data changed ?
            SaveServer(null);
        }


        private void AddServer(List<Shadowsocks> serverList, out int added, out int updated)
        {
            added = 0;
            updated = 0;

            foreach (Shadowsocks server in serverList)
            {
                Shadowsocks serverOld = ShadowsocksOC.FirstOrDefault(predicate: x => x.IsServerEqual(server));
                if (serverOld == null)
                {
                    ShadowsocksOC.Add(server);
                    ++added;
                }
                else
                {
                    if (SettingManager.Configuration.IsReplaceOldServer)
                    {
                        int i = ShadowsocksOC.IndexOf(serverOld);
                        ShadowsocksOC[i] = server;
                        ++updated;
                    }
                }
            }
        }

        private void AddServer(Shadowsocks server, out int added, out int updated)
        {
            added = 0;
            updated = 0;

            Shadowsocks serverOld = ShadowsocksOC.FirstOrDefault(predicate: x => x.IsServerEqual(server));
            if (serverOld == null)
            {
                ShadowsocksOC.Add(server);
                ++added;
            }
            else
            {
                if (SettingManager.Configuration.IsReplaceOldServer)
                {
                    int i = ShadowsocksOC.IndexOf(serverOld);
                    ShadowsocksOC[i] = server;
                    ++updated;
                }
            }
        }

        private bool IsServerFree(object parameter)
        {
            if (parameter is Shadowsocks server)
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
            return !processing_update_info && !processing_check_ping;
        }

        #region Commands
        // save data
        public RelayCommand CommandSaveServer => new RelayCommand(SaveServer);
        private void SaveServer(object parameter)
        {
            // convert to list and save
            List<Shadowsocks> profiles = new List<Shadowsocks>(ShadowsocksOC);
            ServerManager.Save(profiles);
        }

        // change server
        public RelayCommand CommandChangeServer => new RelayCommand(ChangeServer, IsServerFree);
        private void ChangeServer(object parameter)
        {
            if (parameter is Shadowsocks server)
            {
                TransmitCtrl.ChangeTransmitServer(server);
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

            List<Shadowsocks> serverList = Shadowsocks.ImportServers(ssBase64);
            if (serverList.Count > 0)
            {
                AddServer(serverList, out int added, out int updated);

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
            List<Shadowsocks> serverList = Shadowsocks.ImportServers(Clipboard.GetText(TextDataFormat.Text));
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

            List<Shadowsocks> serverList = Shadowsocks.ImportServers(fileContent);
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
            Shadowsocks server = new Shadowsocks();

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
            Shadowsocks serverOld = (Shadowsocks)parameter;
            Shadowsocks serverNew = serverOld.Copy();

            new ServerConfigDialog(serverNew,
                (bool resultOK) =>
                {
                    if (resultOK)
                    {
                        int i = ShadowsocksOC.IndexOf(serverOld);
                        if (i >= 0)
                        {
                            ShadowsocksOC[i] = serverNew;
                        }
                    }
                }).ShowDialog();
        }

        // delete selected server(s)
        public RelayCommand CommandDeleteServers => new RelayCommand(DeleteServers, CanDeleteServer);
        private bool CanDeleteServer(object parameter)
        {
            return (ShadowsocksOC.Count > 0)
                && parameter is System.Collections.IList
                && CanEditList(null);
        }
        private void DeleteServers(object parameter)
        {
            /** https://stackoverflow.com/a/14852516
             */
            System.Collections.IList selection = parameter as System.Collections.IList;
            List<Shadowsocks> servers = selection.Cast<Shadowsocks>().ToList();

            foreach (Shadowsocks server in servers)
            {
                ShadowsocksOC.Remove(server);
            }
        }
        #endregion

        #region Command-UpdateName
        // update name ip information for all servers
        public RelayCommand CommandUpdateInfoAll => new RelayCommand(UpdateInfoAll, CanUpdateInfoAll);

        private bool CanUpdateInfoAll(object parameter) => !processing_update_info;

        private async void UpdateInfoAll(object parameter)
        {
            // ask if force mode
            bool confirm = false;
            Dictionary<string, Action> actions = new Dictionary<string, Action>
            {
                { sr_yes, () => { confirm = true; } },
                { sr_cancel, null },
            };
            DialogAction dialog = new DialogAction(sr_update_info, sr_update_info_confirm, actions);
            dialog.ShowDialog();
            if (!confirm)
            {
                return;
            }

            // add task
            processing_update_info = true;
            TaskView task = new TaskView
            {
                Name = sr_update_info,
                StopAction = () => { processing_update_info = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            // run
            for (int i = 0; i < ShadowsocksOC.Count; ++i)
            {
                // cancel task
                if (processing_update_info == false)
                {
                    break;
                }

                await Task.Run(() => ShadowsocksOC[i].SetFriendNameByIPInfo()).ConfigureAwait(true);
                task.Progress100 = ++i * 100 / ShadowsocksOC.Count;
            }

            // also update interface
            InterfaceCtrl.UpdateHomeTransmitStatue();

            // done
            processing_update_info = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }

        // update name by ip information for selected servers
        public RelayCommand CommandUpdateInfoSelected => new RelayCommand(UpdateInfoSelected, CanUpdateInfoSelected);

        private bool CanUpdateInfoSelected(object parameter)
        {
            return processing_update_info == false && parameter is System.Collections.IList;
        }

        private async void UpdateInfoSelected(object parameter)
        {
            // add task
            processing_update_info = true;
            TaskView task = new TaskView
            {
                Name = sr_update_info,
                StopAction = () => { processing_update_info = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            // check selection items
            IEnumerator<Shadowsocks> enumerator = (parameter as System.Collections.IList).Cast<Shadowsocks>().GetEnumerator();
            List<Shadowsocks> selection = new List<Shadowsocks>();
            while (enumerator.MoveNext())
            {
                selection.Add(enumerator.Current);
            }

            for (int i = 0; i < selection.Count; ++i)
            {
                if (!processing_update_info)
                {
                    break;
                }

                await Task.Run(() => selection[i].SetFriendNameByIPInfo()).ConfigureAwait(true);
                task.Progress100 = i * 100 / selection.Count;
            }

            // also update interface
            InterfaceCtrl.UpdateHomeTransmitStatue();

            // done
            processing_update_info = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion
    }
}
