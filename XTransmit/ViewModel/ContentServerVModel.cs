using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XTransmit.Control;
using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.Utility;
using XTransmit.View;
using XTransmit.ViewModel.Element;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace XTransmit.ViewModel
{
    /**
     * TODO - Column display option
     * TODO - Optimize task cancellation
     */
    class ContentServerVModel : BaseViewModel
    {
        public ObservableCollection<ServerProfile> OCServerProfile { get; private set; }

        // languages
        private static readonly string sr_yes = (string)Application.Current.FindResource("_yes");
        private static readonly string sr_no = (string)Application.Current.FindResource("_no");
        private static readonly string sr_cancel = (string)Application.Current.FindResource("_cancel");

        private static readonly string sr_task_ping_server = (string)Application.Current.FindResource("task_ping_server");
        private static readonly string sr_task_fetch_info = (string)Application.Current.FindResource("task_fetch_info");
        private static readonly string sr_task_check_response_time = (string)Application.Current.FindResource("task_check_response_time");

        private static readonly string sr_config_0_found = (string)Application.Current.FindResource("server_config_0_found");
        private static readonly string sr_config_exist = (string)Application.Current.FindResource("server_config_exist");
        private static readonly string sr_config_x_imported = (string)Application.Current.FindResource("server_config_x_imported");
        private static readonly string sr_config_0_imported = (string)Application.Current.FindResource("server_config_0_imported");
        private static readonly string sr_config_x_added = (string)Application.Current.FindResource("server_config_x_added");

        private static readonly string sr_ask_keep_info_title = (string)Application.Current.FindResource("server_ask_keep_info_title");
        private static readonly string sr_ask_keep_info_message = (string)Application.Current.FindResource("server_ask_keep_info_message");

        public ContentServerVModel()
        {
            ServerManager.Load(App.FileServerXml);

            // load servers and convert to ObservableCollection
            OCServerProfile = new ObservableCollection<ServerProfile>(ServerManager.ServerList);
        }

        ~ContentServerVModel()
        {
            // cancel tasks
            processing_fetch_info = false;
            processing_fetch_response_time = false;
            processing_check_ping = false;

            // data changed ?
            SaveServer(null);
        }


        /** Commands =========================================================================================================
         */
        private volatile bool processing_fetch_info = false; // also use to cancel task
        private volatile bool processing_fetch_response_time = false; // also use to cancel task
        private volatile bool processing_check_ping = false;  // also use to cancel task

        public bool CanEditList(object parameter)
        {
            return !processing_fetch_info && !processing_fetch_response_time && !processing_check_ping;
        }

        private int AddServer(List<ServerProfile> serverList)
        {
            int added = 0;
            foreach (ServerProfile server in serverList)
            {
                ServerProfile serverOld = OCServerProfile.FirstOrDefault(predicate: x => x.IsServerEqual(server));
                if (serverOld == null)
                {
                    OCServerProfile.Add(server);
                    ++added;
                }
                else
                {
                    if (ConfigManager.Global.IsReplaceOldServer)
                    {
                        int i = OCServerProfile.IndexOf(serverOld);
                        OCServerProfile[i] = server;
                        ++added;
                    }
                }
            }

            return added;
        }

        private int AddServer(ServerProfile server)
        {
            int added = 0;

            ServerProfile serverOld = OCServerProfile.FirstOrDefault(predicate: x => x.IsServerEqual(server));
            if (serverOld == null)
            {
                OCServerProfile.Add(server);
                ++added;
            }
            else
            {
                if (ConfigManager.Global.IsReplaceOldServer)
                {
                    int i = OCServerProfile.IndexOf(serverOld);
                    OCServerProfile[i] = server;
                    ++added;
                }
            }

            return added;
        }

        private bool IsServerNotInUse(object parameter)
        {
            if (parameter is ServerProfile server)
            {
                if (!server.IsServerEqual(ConfigManager.Global.RemoteServer))
                {
                    return true;
                }
            }
            return false;
        }

        // save data
        public RelayCommand CommandSaveServer => new RelayCommand(SaveServer);
        private void SaveServer(object parameter)
        {
            // convert to list and save
            List<ServerProfile> profiles = new List<ServerProfile>(OCServerProfile);
            ServerManager.Save(profiles);
        }

        // ipinfo
        public RelayCommand CommandFetchInfo => new RelayCommand(FetchServerInfo, CanFetchInfo);
        private bool CanFetchInfo(object parameter) => !processing_fetch_info;
        private async void FetchServerInfo(object parameter)
        {
            bool? force = null;
            Dictionary<string, Action> actions = new Dictionary<string, Action>
            {
                {
                    sr_yes,
                    () => { force = false; }
                },

                {
                    sr_no,
                    () => { force = true; }
                },

                {
                    sr_cancel,
                    null
                },
            };
            DialogAction dialog = new DialogAction(sr_ask_keep_info_title, sr_ask_keep_info_message, actions);
            dialog.ShowDialog();
            if (force == null)
            {
                return;
            }

            processing_fetch_info = true;
            TaskView task = new TaskView
            {
                Name = sr_task_fetch_info,
                StopAction = () => { processing_fetch_info = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            await Task.Run(() =>
            {
                for (int i = 0; i < OCServerProfile.Count; ++i)
                {
                    // isFetchInProcess is also use to cancel task
                    if (processing_fetch_info == false)
                    {
                        break;
                    }

                    OCServerProfile[i].UpdateIPInfo((bool)force);
                    task.Progress100 = (i * 100 / OCServerProfile.Count) + 1;
                }
            }).ConfigureAwait(true);

            ServerProfile serverSelected = OCServerProfile.FirstOrDefault(x => x.IsServerEqual(ConfigManager.Global.RemoteServer));
            if (serverSelected != null)
            {
                ConfigManager.Global.RemoteServer = serverSelected;
                InterfaceCtrl.UpdateHomeTransmitStatue();
            }

            processing_fetch_info = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }

        // response time
        public RelayCommand CommandFetchResponseTime => new RelayCommand(FetchResponseTime, CanFetchResponseTime);
        private bool CanFetchResponseTime(object parameter) => !processing_fetch_response_time;
        private async void FetchResponseTime(object parameter)
        {
            processing_fetch_response_time = true;
            TaskView task = new TaskView
            {
                Name = sr_task_check_response_time,
                StopAction = () => { processing_fetch_response_time = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            await Task.Run(() =>
            {
                for (int i = 0; i < OCServerProfile.Count; ++i)
                {
                    // isFetchInProcess is also use to cancel task
                    if (processing_fetch_response_time == false)
                    {
                        break;
                    }

                    ServerProfile server = OCServerProfile[i];
                    if (ServerManager.ServerProcessMap.ContainsKey(server))
                    {
                        server.UpdateResponseTime();
                    }
                    else
                    {
                        int listen = NetworkUtil.GetAvailablePort(10000);
                        if (listen > 0)
                        {
                            ServerManager.Start(server, listen);
                            server.UpdateResponseTime();
                            ServerManager.Stop(server);
                        }
                    }

                    task.Progress100 = (i * 100 / OCServerProfile.Count) + 1;
                }
            }).ConfigureAwait(true);

            processing_fetch_response_time = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }

        // ping 
        public RelayCommand CommandCheckPing => new RelayCommand(CheckPing, CanCheckPing);
        private bool CanCheckPing(object parameter) => !processing_check_ping;

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async void CheckPing(object parameter)
        {
            processing_check_ping = true;
            TaskView task = new TaskView
            {
                Name = sr_task_ping_server,
                StopAction = () => { processing_check_ping = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            int timeout = ConfigManager.Global.PingTimeout;
            using (Ping ping = new Ping())
            {
                for (int i = 0; i < OCServerProfile.Count; ++i)
                {
                    // isPingInProcess is also use to cancel task
                    if (processing_check_ping == false)
                    {
                        break;
                    }

                    ServerProfile server = OCServerProfile[i];
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(server.HostIP, timeout).ConfigureAwait(true);
                        server.Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                    }
                    catch (Exception)
                    {
                        server.Ping = -1;
                    }

                    task.Progress100 = (i * 100 / OCServerProfile.Count) + 1;
                }
            }

            processing_check_ping = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }

        // select server, not in use
        public RelayCommand CommandSelectServer => new RelayCommand(SelectServer, IsServerNotInUse);
        private void SelectServer(object parameter)
        {
            if (parameter is ServerProfile server)
            {
                // Set ServerProfile
                TransmitCtrl.ChangeTransmitServer(server);
            }
        }

        // add server by scan qrcode
        // TODO Fix - Some QRCode can not be recognized
        public RelayCommand CommandAddServerQRCode => new RelayCommand(AddServerQRCode, CanEditList);
        private void AddServerQRCode(object parameter)
        {
            // copy screen
            Bitmap bitmapScreen = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmapScreen))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmapScreen.Size, CopyPixelOperation.SourceCopy);
            }

            BitmapLuminanceSource sourceScreen = new BitmapLuminanceSource(bitmapScreen);
            BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(sourceScreen));
            bitmapScreen.Dispose();

            QRCodeReader reader = new QRCodeReader();
            Result result = reader.decode(bitmap);
            if (result == null || string.IsNullOrWhiteSpace(result.Text))
            {
                InterfaceCtrl.ShowHomeNotify(sr_config_0_found);
                InterfaceCtrl.NotifyIcon.ShowMessage(sr_config_0_found);
                return;
            }

            List<ServerProfile> serverList = ServerManager.ImportServers(result.Text);
            if (serverList.Count > 0)
            {
                int added = AddServer(serverList);
                string notify = added > 0 ? $"{added} {sr_config_x_imported}" : sr_config_exist;

                InterfaceCtrl.ShowHomeNotify(notify);
                InterfaceCtrl.NotifyIcon.ShowMessage(notify);
            }
            else
            {
                InterfaceCtrl.ShowHomeNotify(sr_config_0_imported);
                InterfaceCtrl.NotifyIcon.ShowMessage(sr_config_0_imported);
            }
        }

        // add server by clipboard import
        public RelayCommand CommandAddServerClipboard => new RelayCommand(AddServerClipboard, CanEditList);
        private void AddServerClipboard(object parameter)
        {
            List<ServerProfile> serverList = ServerManager.ImportServers(Clipboard.GetText(TextDataFormat.Text));
            if (serverList.Count > 0)
            {
                int added = AddServer(serverList);
                string notify = added > 0 ? $"{added} {sr_config_x_imported}" : sr_config_exist;

                InterfaceCtrl.ShowHomeNotify(notify);
            }
            else
            {
                InterfaceCtrl.ShowHomeNotify(sr_config_0_imported);
            }
        }

        // add server by file import
        public RelayCommand CommandAddServerFile => new RelayCommand(AddServerFile, CanEditList);
        private void AddServerFile(object parameter)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = App.PathCurrent,
                DefaultExt = "txt",
                Filter = "Text|*.txt",
                AddExtension = true
            };

            bool? open = openFileDialog.ShowDialog();
            if (open != true) return;

            List<ServerProfile> serverList = ServerManager.ImportServers(openFileDialog.FileName);
            if (serverList.Count > 0)
            {
                int added = AddServer(serverList);
                string notify = added > 0 ? $"{added} {sr_config_x_imported}" : sr_config_exist;

                InterfaceCtrl.ShowHomeNotify(notify);
            }
            else
            {
                InterfaceCtrl.ShowHomeNotify(sr_config_0_imported);
            }
        }

        // add server by manual create
        public RelayCommand CommandAddServerNew => new RelayCommand(AddServerNew, CanEditList);
        private void AddServerNew(object parameter)
        {
            ServerProfile server = new ServerProfile();

            new DialogServerConfig(server,
                (bool resultOK) =>
                {
                    if (resultOK)
                    {
                        int added = AddServer(server);
                        string notify = added > 0 ? $"{added} {sr_config_x_added}" : sr_config_exist;

                        InterfaceCtrl.ShowHomeNotify(notify);
                    }
                }).ShowDialog();
        }

        // edit server, not in use
        public RelayCommand CommandEditServer => new RelayCommand(EditServer, IsServerNotInUse);
        private void EditServer(object parameter)
        {
            ServerProfile serverOld = (ServerProfile)parameter;
            ServerProfile serverNew = serverOld.Copy();

            new DialogServerConfig(serverNew,
                (bool resultOK) =>
                {
                    if (resultOK)
                    {
                        int i = OCServerProfile.IndexOf(serverOld);
                        if (i >= 0)
                        {
                            OCServerProfile[i] = serverNew;
                        }
                    }
                }).ShowDialog();
        }

        // delete selected server(s), not in use
        public RelayCommand CommandDeleteServers => new RelayCommand(DeleteServers, CanDeleteServer);
        private bool CanDeleteServer(object serverSelected)
        {
            return (OCServerProfile.Count > 0) && CanEditList(null);
        }
        private void DeleteServers(object serversSelected)
        {
            /** https://stackoverflow.com/a/14852516
             */
            System.Collections.IList selected = serversSelected as System.Collections.IList;
            List<ServerProfile> listServerProfile = selected.Cast<ServerProfile>().ToList();

            foreach (ServerProfile server in listServerProfile)
            {
                OCServerProfile.Remove(server);
            }
        }
    }
}
