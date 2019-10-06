using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XTransmit.Model.Server;
using XTransmit.Utility;
using XTransmit.View;
using XTransmit.ViewModel.Model;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace XTransmit.ViewModel
{
    /**TODO - Column display option
     * Updated: 2019-10-05
     */
    class ContentServerVModel : BaseViewModel
    {
        public ObservableCollection<ServerView> ServerViewListOC { get; private set; }

        // languages
        private static readonly string sr_config_0_found = (string)Application.Current.FindResource("server_config_0_found");
        private static readonly string sr_config_x_imported = (string)Application.Current.FindResource("server_config_x_imported");
        private static readonly string sr_config_0_imported = (string)Application.Current.FindResource("server_config_0_imported");
        private static readonly string sr_config_x_added = (string)Application.Current.FindResource("server_config_x_added");
        private static readonly string sr_ask_keep_info_title = (string)Application.Current.FindResource("server_ask_keep_info_title");
        private static readonly string sr_ask_keep_info_message = (string)Application.Current.FindResource("server_ask_keep_info_message");

        public ContentServerVModel()
        {
            ServerManager.Load(App.FileServerXml);

            // load servers and convert to ObservableCollection
            ServerViewListOC = new ObservableCollection<ServerView>();
            foreach (ServerProfile server in ServerManager.ServerList)
            {
                ServerViewListOC.Add(new ServerView(server));
            }

            // save data on closing
            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // case "hide" 
            if (Application.Current.MainWindow.IsVisible)
            {
                return;
            }

            // cancel task
            processig_fetch_info = false;
            processing_fetch_response_time = false;
            processing_check_ping = false;

            // data changed ?
            SaveServer(null);
        }


        /** Commands =========================================================================================================
         */
        private volatile bool processig_fetch_info = false; // also use to cancel task
        private volatile bool processing_fetch_response_time = false; // also use to cancel task
        private volatile bool processing_check_ping = false;  // also use to cancel task

        private int AddServer(List<ServerProfile> serverList)
        {
            int added = 0;
            foreach (ServerProfile server in serverList)
            {
                // no duplicates
                if (ServerViewListOC.FirstOrDefault(predicate: x => x.vServerProfile.Equals(server)) == null)
                {
                    ServerViewListOC.Add(new ServerView(server));
                    ++added;
                }
            }

            return added;
        }

        private int AddServer(ServerProfile server)
        {
            int added = 0;
            if (ServerViewListOC.FirstOrDefault(predicate: x => x.vServerProfile.Equals(server)) == null)
            {
                ServerViewListOC.Add(new ServerView(server));
                ++added;
            }

            return added;
        }

        private bool IsServerNotInUse(object serverNew)
        {
            if (serverNew is ServerView serverInfo)
            {
                if (!serverInfo.vServerProfile.Equals(App.GlobalConfig.RemoteServer))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanEditList(object parameter)
        {
            return !processig_fetch_info && !processing_fetch_response_time && !processing_check_ping;
        }

        // save data
        public RelayCommand CommandSaveServer => new RelayCommand(SaveServer);
        private void SaveServer(object parameter)
        {
            // convert to list and save
            List<ServerProfile> profiles = new List<ServerProfile>();
            foreach (ServerView serverView in ServerViewListOC)
            {
                profiles.Add(serverView.vServerProfile);
            }

            ServerManager.Save(profiles);
        }

        // ipinfo
        public RelayCommand CommandFetchInfo => new RelayCommand(FetchServerInfo, CanFetchInfo);
        private bool CanFetchInfo(object parameter) => !processig_fetch_info;
        private async void FetchServerInfo(object parameter)
        {
            DialogAction dialog = new DialogAction(sr_ask_keep_info_title, sr_ask_keep_info_message);
            dialog.ShowDialog();
            if (!(dialog.CancelableResult is bool keep))
            {
                return;
            }

            processig_fetch_info = true;
            App.UpdateProgress(40);

            await Task.Run(() =>
            {
                foreach (ServerView serverView in ServerViewListOC)
                {
                    // isFetchInProcess is also use to cancel task
                    if (processig_fetch_info == false)
                    {
                        break;
                    }

                    serverView.UpdateIPInfo(!keep);
                }
            });

            // it will update the server info only if the server is not changed
            ServerView serverSelected = ServerViewListOC.FirstOrDefault(x => x.vServerProfile.Equals(App.GlobalConfig.RemoteServer));
            if (serverSelected != null)
            {
                App.ChangeTransmitServer(serverSelected.vServerProfile);
            }

            processig_fetch_info = false;
            App.UpdateProgress(-40);
            CommandManager.InvalidateRequerySuggested();
        }

        // response time
        public RelayCommand CommandFetchResponseTime => new RelayCommand(FetchResponseTime, CanFetchResponseTime);
        private bool CanFetchResponseTime(object parameter) => !processing_fetch_response_time;
        private async void FetchResponseTime(object parameter)
        {
            processing_fetch_response_time = true;
            App.UpdateProgress(40);

            await Task.Run(() =>
            {
                foreach (ServerView serverView in ServerViewListOC)
                {
                    // isFetchInProcess is also use to cancel task
                    if (processing_fetch_response_time == false)
                    {
                        // TODO - Break
                        return;
                    }

                    if (ServerManager.ServerProcessMap.ContainsKey(serverView.vServerProfile))
                    {
                        serverView.UpdateResponseTime();
                    }
                    else
                    {
                        int listen = NetworkUtil.GetAvailablePort(10000);
                        if (listen > 0)
                        {
                            ServerManager.Start(serverView.vServerProfile, listen);
                            serverView.UpdateResponseTime();
                            ServerManager.Stop(serverView.vServerProfile);
                        }
                    }
                }
            });

            processing_fetch_response_time = false;
            App.UpdateProgress(-40);
            CommandManager.InvalidateRequerySuggested();
        }

        // ping 
        public RelayCommand CommandCheckPing => new RelayCommand(CheckPing, CanCheckPing);
        private bool CanCheckPing(object parameter) => !processing_check_ping;
        private async void CheckPing(object parameter)
        {
            processing_check_ping = true;
            App.UpdateProgress(40);

            int timeout = App.GlobalConfig.PingTimeout;
            using (Ping ping = new Ping())
            {
                foreach (ServerView serverView in ServerViewListOC)
                {
                    // isPingInProcess is also use to cancel task
                    if (processing_check_ping == false)
                    {
                        return;
                    }

                    try
                    {
                        PingReply reply = await ping.SendPingAsync(serverView.HostIP, timeout);
                        serverView.Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                    }
                    catch (Exception)
                    {
                        serverView.Ping = -1;
                    }
                }
            }

            processing_check_ping = false;
            App.UpdateProgress(-40);
            CommandManager.InvalidateRequerySuggested();
        }

        // select server, not in use
        public RelayCommand CommandSelectServer => new RelayCommand(SelectServer, IsServerNotInUse);
        private void SelectServer(object serverNew)
        {
            if (serverNew is ServerView serverView)
            {
                // Set ServerProfile
                App.ChangeTransmitServer(serverView.vServerProfile);
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

            QRCodeReader reader = new QRCodeReader();
            Result result = reader.decode(bitmap);
            if (result == null || string.IsNullOrWhiteSpace(result.Text))
            {
                App.ShowNotify(sr_config_0_found);
                return;
            }

            List<ServerProfile> serverList = ServerManager.ImportServers(result.Text);
            if (serverList.Count > 0)
            {
                int added = AddServer(serverList);
                App.ShowNotify($"{added} {sr_config_x_imported}");
            }
            else
            {
                App.ShowNotify(sr_config_0_imported);
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
                App.ShowNotify($"{added} {sr_config_x_imported}");
            }
            else
            {
                App.ShowNotify(sr_config_0_imported);
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
                App.ShowNotify($"{added} {sr_config_x_imported}");
            }
            else
            {
                App.ShowNotify(sr_config_0_imported);
            }
        }

        // add server by manual create
        public RelayCommand CommandAddServerNew => new RelayCommand(AddServerNew, CanEditList);
        private void AddServerNew(object parameter)
        {
            ServerProfile server = new ServerProfile();
            if (new DialogServerConfig(server).ShowDialog() is bool update && update == true)
            {
                int added = AddServer(server);
                App.ShowNotify($"{added} {sr_config_x_added}");
            }
        }

        // edit server, not in use
        public RelayCommand CommandEditServer => new RelayCommand(EditServer, IsServerNotInUse);
        private void EditServer(object serverSelected)
        {
            ServerView serverView = (ServerView)serverSelected;
            ServerProfile serverProfile = serverView.vServerProfile.Copy();

            if (new DialogServerConfig(serverProfile).ShowDialog() is bool update && update == true)
            {
                int index = ServerViewListOC.IndexOf(serverView);
                if (index >= 0)
                {
                    ServerViewListOC[index] = new ServerView(serverProfile);
                }
            }
        }

        // delete selected server(s), not in use
        public RelayCommand CommandDeleteServers => new RelayCommand(DeleteServers, CanDeleteServer);
        private bool CanDeleteServer(object serverSelected)
        {
            return (ServerViewListOC.Count > 0) && CanEditList(null);
        }
        private void DeleteServers(object serversSelected)
        {
            /** https://stackoverflow.com/a/14852516
             */
            System.Collections.IList selected = serversSelected as System.Collections.IList;
            List<ServerView> serverViewList = selected.Cast<ServerView>().ToList();

            foreach (ServerView serverView in serverViewList)
            {
                ServerViewListOC.Remove(serverView);
            }
        }
    }
}
