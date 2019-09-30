using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.Utility;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-09-30
     */
    public class HomeVModel : BaseViewModel
    {
        public bool IsTransmitEnabled
        {
            get => App.GlobalConfig.IsTransmitEnabled;
            set
            {
                if (value)
                {
                    TransmitEnable();
                }
                else
                {
                    TransmitDisable();
                }
            }
        }

        public bool IsServerPoolEnabled
        {
            get => App.GlobalConfig.IsServerPoolEnabled;
            set
            {
                if (value)
                {
                    StartServerPool();
                }
                else
                {
                    StopServerPool();
                }
            }
        }
        
        public string TransmitStatus
        {
            get
            {
                if (App.GlobalConfig.IsServerPoolEnabled)
                {
                    return $"{SSManager.SSProcessMap.Count}";
                }
                else
                {
                    return App.GlobalConfig.RemoteServer != null ?
                        App.GlobalConfig.RemoteServer.FriendlyName : sr_server_not_set;
                }
            }
        }

        // progress
        public ProgressInfo Progress { get; private set; }

        // table
        public UserControl ContentDisplay { get; private set; }
        public List<ContentTable> ContentList { get; private set; }

        private static readonly string sr_server_not_set = (string)Application.Current.FindResource("server_not_set");

        public HomeVModel()
        {
            // init progress
            Progress = new ProgressInfo(0, false);

            // init content list and display
            ContentList = new List<ContentTable>
            {
                new ContentTable("Server", new View.ContentServer()),
                new ContentTable("X-CURL", new View.ContentCurl()),
                new ContentTable("Netwrok", new View.ContentNetwork()),
            };

            // TODO - DragDrop table
            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.Title == App.GlobalPreference.ContentDisplay);
            if (contentTable == null)
            {
                contentTable = ContentList[0];
            }

            contentTable.IsChecked = true;
            ContentDisplay = contentTable.Content;

            // transmit control. Trigge the set
            IsTransmitEnabled = App.GlobalConfig.IsTransmitEnabled;
            App.GlobalConfig.IsServerPoolEnabled = false;

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

            // save preference
            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.IsChecked);
            App.GlobalPreference.ContentDisplay = contentTable.Title;
        }

        private void TransmitEnable()
        {
            Config config = App.GlobalConfig;
            if (config.RemoteServer == null)
            {
                return;
            }

            if (config.SystemProxyPort == 0)
            {
                config.SystemProxyPort = NetworkUtil.GetAvailablePort(2000);
            }
            else
            {
                List<int> portInUse = NetworkUtil.GetPortInUse(2000);
                if (portInUse.Contains(config.SystemProxyPort))
                {
                    config.SystemProxyPort = NetworkUtil.GetAvailablePort(2000, portInUse);
                }
            }
            NativeMethods.EnableProxy($"127.0.0.1:{config.SystemProxyPort}", NativeMethods.Bypass);

            if (config.GlobalSocks5Port == 0)
            {
                config.GlobalSocks5Port = NetworkUtil.GetAvailablePort(2000);
            }
            else
            {
                List<int> portInUse = NetworkUtil.GetPortInUse(2000);
                if (portInUse.Contains(config.GlobalSocks5Port))
                {
                    config.GlobalSocks5Port = NetworkUtil.GetAvailablePort(2000, portInUse);
                }
            }

            PrivoxyManager.Start(config.SystemProxyPort, config.GlobalSocks5Port);

            if (config.RemoteServer != null)
            {
                SSManager.Start(config.RemoteServer, config.GlobalSocks5Port);
            }

            App.GlobalConfig.IsTransmitEnabled = true;
            OnPropertyChanged("IsTransmitEnabled"); //from notifyicon
        }
        private void TransmitDisable()
        {
            NativeMethods.DisableProxy();
            PrivoxyManager.Stop();
            SSManager.Stop(App.GlobalConfig.RemoteServer); // server pool

            App.GlobalConfig.IsTransmitEnabled = false;
            OnPropertyChanged("IsTransmitEnabled"); //from notifyicon
        }

        /** Server Pool 
         */
        private void StartServerPool()
        {
            foreach (ServerProfile server in ServerManager.ServerList)
            {
                int listen = NetworkUtil.GetAvailablePort(2000);
                if (listen > 0)
                {
                    SSManager.Start(server, listen);
                }
            }

            // ToggleButton auto update "Checked"(server_pool_enabled) properity
            App.GlobalConfig.IsServerPoolEnabled = true;
            OnPropertyChanged("TransmitStatus");
        }

        private void StopServerPool()
        {
            // "push" transmit status
            if (App.GlobalConfig.IsTransmitEnabled)
            {
                ServerManager.ServerList.Remove(App.GlobalConfig.RemoteServer);
            }

            foreach (ServerProfile server in ServerManager.ServerList)
            {
                SSManager.Stop(server);
            }

            // "pop" transmit status
            if (App.GlobalConfig.IsTransmitEnabled)
            {
                ServerManager.ServerList.Add(App.GlobalConfig.RemoteServer);
            }

            App.GlobalConfig.IsServerPoolEnabled = false;
            OnPropertyChanged("TransmitStatus");
        }

        /** actoins ====================================================================================================== 
         */
        // a functional interface
        public void AddServerByScanQRCode()
        {
            // TODO - take care of the ContentTables order
            ContentServerVModel serverViewModel = (ContentServerVModel)ContentList[0].Content.DataContext;
            serverViewModel.CommandAddServerQRCode.Execute(null);
        }

        // Progress is indeterminated, This mothod increase/decrease the progress value.
        // TODO Next - Progress list
        public void UpdateProgress(int progress)
        {
            Progress.Value += progress;
            if (Progress.Value < 0) Progress.Value = 0;

            if (Progress.Value == 0) Progress.IsIndeterminate = false;
            else Progress.IsIndeterminate = true;

            OnPropertyChanged("Progress");
        }

        public void UpdateTransmitServer(ServerProfile serverProfile)
        {
            if (!App.GlobalConfig.RemoteServer.Equals(serverProfile))
            {
                if (App.GlobalConfig.IsTransmitEnabled)
                {
                    SSManager.Stop(App.GlobalConfig.RemoteServer);
                    SSManager.Start(serverProfile, App.GlobalConfig.GlobalSocks5Port);
                }
            }

            App.GlobalConfig.RemoteServer = serverProfile;
            OnPropertyChanged("RemoteServerName");
        }


        /** Commands ======================================================================================================
         */
        public RelayCommand CommandSwitchContent => new RelayCommand(SwitchContent);
        private void SwitchContent(object newTitle)
        {
            if (newTitle is string title)
            {
                UserControl content = ContentList.FirstOrDefault(x => x.Title == title).Content;
                if (ContentDisplay != content)
                {
                    ContentDisplay = content;
                    OnPropertyChanged("ContentDisplay");
                }
            }
        }

        // show setting
        public RelayCommand CommandShowSetting => new RelayCommand(ShowSetting);
        private void ShowSetting(object parameter)
        {
            new View.DialogSetting().ShowDialog();
        }

        // show about
        public RelayCommand CommandShowAbout => new RelayCommand(ShowAbout);
        private void ShowAbout(object parameter)
        {
            new View.DialogAbout().ShowDialog();
        }

        // exit
        public RelayCommand CommandExit => new RelayCommand(ExitApp);
        private void ExitApp(object parameter)
        {
            App.CloseMainWindow();
        }
    }
}
