﻿using System;
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

        // status
        private volatile bool processing_fetch_info = false;

        // language
        private static readonly string sr_yes = (string)Application.Current.FindResource("_yes");
        private static readonly string sr_no = (string)Application.Current.FindResource("_no");
        private static readonly string sr_cancel = (string)Application.Current.FindResource("_cancel");

        private static readonly string sr_server_not_found = (string)Application.Current.FindResource("add_server_not_found");
        private static readonly string sr_server_already_exist = (string)Application.Current.FindResource("add_server_already_exist");
        private static readonly string sr_server_x_added = (string)Application.Current.FindResource("add_server_x_added");
        private static readonly string sr_server_x_updated = (string)Application.Current.FindResource("add_server_x_updated");

        private static readonly string sr_task_fetch_info = (string)Application.Current.FindResource("task_fetch_info");
        private static readonly string sr_fetch_ask_focus_title = (string)Application.Current.FindResource("server_fetch_info");
        private static readonly string sr_fetch_ask_focus_message = (string)Application.Current.FindResource("server_fetch_ask_force");

        public ContentShadowsocksVModel()
        {
            // load servers and convert to ObservableCollection
            ShadowsocksOC = new ObservableCollection<Shadowsocks>(ServerManager.ShadowsocksList);
            Servers = ShadowsocksOC.Cast<BaseServer>();
        }

        ~ContentShadowsocksVModel()
        {
            // cancel tasks
            processing_fetch_info = false;
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
            return !processing_fetch_info && !processing_check_ping;
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
        private bool CanDeleteServer(object serverSelected)
        {
            return (ShadowsocksOC.Count > 0) && CanEditList(null);
        }
        private void DeleteServers(object serversSelected)
        {
            /** https://stackoverflow.com/a/14852516
             */
            System.Collections.IList selected = serversSelected as System.Collections.IList;
            List<Shadowsocks> listServerProfile = selected.Cast<Shadowsocks>().ToList();

            foreach (Shadowsocks server in listServerProfile)
            {
                ShadowsocksOC.Remove(server);
            }
        }
        #endregion

        #region Command-FetchInfo
        // fetch ip information for all servers
        public RelayCommand CommandFetchInfoAll => new RelayCommand(FetchInfoAll, CanFetchInfoAll);

        private bool CanFetchInfoAll(object parameter) => !processing_fetch_info;

        private async void FetchInfoAll(object parameter)
        {
            // ask if force mode
            bool? force = null;
            Dictionary<string, Action> actions = new Dictionary<string, Action>
            {
                { sr_yes, () => { force = false; } },
                { sr_no, () => { force = true; } },
                { sr_cancel, null },
            };
            DialogAction dialog = new DialogAction(sr_fetch_ask_focus_title, sr_fetch_ask_focus_message, actions);
            dialog.ShowDialog();
            if (force == null)
            {
                return;
            }

            // add task
            processing_fetch_info = true;
            TaskView task = new TaskView
            {
                Name = sr_task_fetch_info,
                StopAction = () => { processing_fetch_info = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            // run
            await Task.Run(() =>
            {
                for (int i = 0; i < ShadowsocksOC.Count; ++i)
                {
                    // cancel task
                    if (processing_fetch_info == false)
                    {
                        break;
                    }

                    ShadowsocksOC[i].UpdateIPInfo((bool)force);
                    task.Progress100 = ++i * 100 / ShadowsocksOC.Count;
                }
            }).ConfigureAwait(true);

            // also update interface
            InterfaceCtrl.UpdateHomeTransmitStatue();

            // done
            processing_fetch_info = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }

        // fetch ip information for selected servers
        public RelayCommand CommandFetchInfoSelected => new RelayCommand(FetchInfoSelected, CanFetchInfoSelected);

        private bool CanFetchInfoSelected(object parameter)
        {
            return processing_fetch_info == false && parameter is Shadowsocks;
        }

        private async void FetchInfoSelected(object parameter)
        {
            Shadowsocks server = (Shadowsocks)parameter;

            // add task
            processing_fetch_info = true;
            TaskView task = new TaskView
            {
                Name = sr_task_fetch_info,
                StopAction = null
            };
            InterfaceCtrl.AddHomeTask(task);

            // run
            await Task.Run(() =>
            {
                server.UpdateIPInfo(true); // force
                task.Progress100 = 100;
            }).ConfigureAwait(true);

            // also update interface
            InterfaceCtrl.UpdateHomeTransmitStatue();

            // done
            processing_fetch_info = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion
    }
}
