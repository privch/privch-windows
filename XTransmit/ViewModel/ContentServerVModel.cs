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
using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.View;
using XTransmit.ViewModel.Model;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace XTransmit.ViewModel
{
    /**TODO - Column display option
     * Updated: 2019-08-06
     */
    class ContentServerVModel : BaseViewModel
    {
        public ObservableCollection<ServerProfileView> ObServerInfoList { get; private set; }
        private readonly Config config;

        // languages
        private static readonly string sr_config_0_found = (string)Application.Current.FindResource("server_config_0_found");
        private static readonly string sr_config_x_imported = (string)Application.Current.FindResource("server_config_x_imported");
        private static readonly string sr_config_0_imported = (string)Application.Current.FindResource("server_config_0_imported");
        private static readonly string sr_config_x_added = (string)Application.Current.FindResource("server_config_x_added");
        private static readonly string sr_ask_keep_info_title = (string)Application.Current.FindResource("server_ask_keep_info_title");
        private static readonly string sr_ask_keep_info_message = (string)Application.Current.FindResource("server_ask_keep_info_message");

        public ContentServerVModel()
        {
            config = App.GlobalConfig;
            ServerManager.Load(App.FileServerXml);

            // load servers and convert to ObservableCollection
            ObServerInfoList = new ObservableCollection<ServerProfileView>();
            foreach (ServerProfile server in ServerManager.ServerList)
            {
                ObServerInfoList.Add(new ServerProfileView(server));
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

            isFetchInProcess = false; // isPingInProcess is also use to cancel task
            isPingInProcess = false;  // isPingInProcess is also use to cancel task

            // convert to list and save
            List<ServerProfileView> infos = new List<ServerProfileView>(ObServerInfoList);
            List<ServerProfile> profiles = new List<ServerProfile>();
            foreach (ServerProfileView info in infos)
            {
                profiles.Add(info.vServerProfile);
            }

            ServerManager.Save(profiles);
        }


        /** Commands =========================================================================================================
         */
        private volatile bool isFetchInProcess = false; // isPingInProcess is also use to cancel task
        private volatile bool isPingInProcess = false;  // isPingInProcess is also use to cancel task

        private int addServer(List<ServerProfile> serverList)
        {
            int added = 0;
            foreach (ServerProfile server in serverList)
            {
                // no duplicates
                if (ObServerInfoList.FirstOrDefault(predicate: x => x.vServerProfile.Equals(server)) == null)
                {
                    ObServerInfoList.Add(new ServerProfileView(server));
                    ++added;
                }
            }

            return added;
        }

        private int addServer(ServerProfile server)
        {
            int added = 0;
            if (ObServerInfoList.FirstOrDefault(predicate: x => x.vServerProfile.Equals(server)) == null)
            {
                ObServerInfoList.Add(new ServerProfileView(server));
                ++added;
            }

            return added;
        }

        private bool isServerNotInUse(object serverNew)
        {
            if (serverNew is ServerProfileView serverInfo)
            {
                if (!serverInfo.vServerProfile.Equals(config.RemoteServer))
                {
                    return true;
                }
            }
            return false;
        }

        // add server by scan qrcode
        // TODO Fix - Some QRCode can not be recognized
        public RelayCommand CommandAddServerQRCode => new RelayCommand(addServerQRCode);
        private void addServerQRCode(object parameter)
        {
            Bitmap bitmapScreen = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmapScreen))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmapScreen.Size, CopyPixelOperation.SourceCopy);
            }

            var sourceScreen = new BitmapLuminanceSource(bitmapScreen);
            var bitmap = new BinaryBitmap(new HybridBinarizer(sourceScreen));

            QRCodeReader reader = new QRCodeReader();
            var result = reader.decode(bitmap);
            if (result == null || string.IsNullOrWhiteSpace(result.Text))
            {
                App.ShowNotify(sr_config_0_found);
                return;
            }

            List<ServerProfile> serverList = ServerManager.ImportServers(result.Text);
            if (serverList.Count > 0)
            {
                int added = addServer(serverList);
                App.ShowNotify($"{added} {sr_config_x_imported}");
            }
            else
            {
                App.ShowNotify(sr_config_0_imported);
            }
        }

        // add server by clipboard import
        public RelayCommand CommandAddServerClipboard => new RelayCommand(addServerClipboard);
        private void addServerClipboard(object parameter)
        {
            List<ServerProfile> serverList = ServerManager.ImportServers(Clipboard.GetText(TextDataFormat.Text));
            if (serverList.Count > 0)
            {
                int added = addServer(serverList);
                App.ShowNotify($"{added} {sr_config_x_imported}");
            }
            else
            {
                App.ShowNotify(sr_config_0_imported);
            }
        }

        // add server by file import
        public RelayCommand CommandAddServerFile => new RelayCommand(addServerFile);
        private void addServerFile(object parameter)
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
                int added = addServer(serverList);
                App.ShowNotify($"{added} {sr_config_x_imported}");
            }
            else
            {
                App.ShowNotify(sr_config_0_imported);
            }
        }

        // add server by manual create
        public RelayCommand CommandAddServerNew => new RelayCommand(addServerNew);
        private void addServerNew(object parameter)
        {
            ServerProfile server = new ServerProfile();
            if (new DialogServerConfig(server).ShowDialog() is bool update && update == true)
            {
                int added = addServer(server);
                App.ShowNotify($"{added} {sr_config_x_added}");
            }
        }

        // select server, not in use
        public RelayCommand CommandSelectServer => new RelayCommand(selectServer, isServerNotInUse);
        private void selectServer(object serverNew)
        {
            ServerProfileView serverInfo = (ServerProfileView)serverNew;

            // Set ServerProfile
            App.UpdateTransmitServer(serverInfo.vServerProfile);
        }

        // edit server, not in use
        public RelayCommand CommandEditServer => new RelayCommand(editServer, isServerNotInUse);
        private void editServer(object serverSelected)
        {
            ServerProfileView serverInfo = (ServerProfileView)serverSelected;
            ServerProfile serverProfile = serverInfo.vServerProfile.Copy();

            if (new DialogServerConfig(serverProfile).ShowDialog() is bool update && update == true)
            {
                int index = ObServerInfoList.IndexOf(serverInfo);
                if (index >= 0)
                {
                    ObServerInfoList[index] = new ServerProfileView(serverProfile);
                }
            }
        }

        // delete selected server(s), not in use
        public RelayCommand CommandDeleteServers => new RelayCommand(deleteServers);
        private void deleteServers(object serversSelected)
        {
            /** https://stackoverflow.com/a/14852516
             */
            System.Collections.IList selected = serversSelected as System.Collections.IList;
            List<ServerProfileView> serverInfoList = selected.Cast<ServerProfileView>().ToList();

            foreach (ServerProfileView serverInfo in serverInfoList)
            {
                ObServerInfoList.Remove(serverInfo);
            }
        }

        // ipinfo
        public RelayCommand CommandFetchInfo => new RelayCommand(fetchServerInfo, fetchNotInProgress);
        private bool fetchNotInProgress(object parameter) => !isFetchInProcess;
        private async void fetchServerInfo(object parameter)
        {
            DialogButton dialog = new DialogButton(sr_ask_keep_info_title, sr_ask_keep_info_message);
            dialog.ShowDialog();
            if (!(dialog.CancelableResult is bool keep))
                return;

            isFetchInProcess = true;
            App.UpdateProgress(40);

            await Task.Run(() =>
            {
                foreach (ServerProfileView serverInfo in ObServerInfoList)
                {
                    // isFetchInProcess is also use to cancel task
                    if (isFetchInProcess == false) return;

                    serverInfo.UpdateIPInfo(!keep);
                }
            });

            try
            {
                // it will update the server info only if the server is not changed
                ServerProfileView serverInfo = ObServerInfoList.First(x => x.HostIP == config.RemoteServer.vHostIP && x.Port == config.RemoteServer.vPort);
                App.UpdateTransmitServer(serverInfo.vServerProfile);
            }
            catch (Exception) { }

            isFetchInProcess = false;
            App.UpdateProgress(-40);
            CommandManager.InvalidateRequerySuggested();
        }

        // ping 
        public RelayCommand CommandPingInfo => new RelayCommand(fetchServerPing, pingNotInProgress);
        private bool pingNotInProgress(object parameter) => !isPingInProcess;
        private async void fetchServerPing(object parameter)
        {
            isPingInProcess = true;
            App.UpdateProgress(40);

            using (Ping ping = new Ping())
            {
                foreach (ServerProfileView serverInfo in ObServerInfoList)
                {
                    // isPingInProcess is also use to cancel task
                    if (isPingInProcess == false) return;

                    try
                    {
                        PingReply reply = await ping.SendPingAsync(serverInfo.HostIP, 2000);
                        serverInfo.Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                    }
                    catch (Exception)
                    {
                        serverInfo.Ping = -1;
                    }
                }
            }

            isPingInProcess = false;
            App.UpdateProgress(-40);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
