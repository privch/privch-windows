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
     * Updated: 2019-08-06
     */
    public class HomeVModel : BaseViewModel
    {
        public bool IsTransmitEnabled
        {
            get => App.GlobalConfig.IsTransmitEnabled;
            set
            {
                if (value) TransmitEnable();
                else TransmitDisable();

                App.GlobalConfig.IsTransmitEnabled = value;
                OnPropertyChanged("IsTransmitEnabled");
            }
        }

        public string RemoteServerName => App.GlobalConfig.RemoteServer != null ?
            App.GlobalConfig.RemoteServer.vFriendlyName : sr_server_not_set;

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

            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.Title == App.GlobalPreference.ContentDisplay);
            if (contentTable == null)
                contentTable = ContentList[0];

            contentTable.IsChecked = true;
            ContentDisplay = contentTable.Content;

            // transmit control
            IsTransmitEnabled = App.GlobalConfig.IsTransmitEnabled;

            // save data on closing
            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // case "hide" 
            if (Application.Current.MainWindow.IsVisible)
                return;

            // save preference
            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.IsChecked);
            App.GlobalPreference.ContentDisplay = contentTable.Title;
        }

        private void TransmitEnable()
        {
            Config config = App.GlobalConfig;
            if (config.RemoteServer == null)
                return;

            if (config.SystemProxyPort == 0)
            {
                config.SystemProxyPort = NetworkUtil.GetAvailablePort(2000);
            }
            else
            {
                var uri = NativeMethods.GetCurrentConfig("https://www.google.com");
                if (uri.Host == "127.0.0.1" && uri.Port == config.SystemProxyPort)
                {
                    config.SystemProxyPort = NetworkUtil.GetAvailablePort(2000);
                }
            }
            NativeMethods.EnableProxy($"127.0.0.1:{config.SystemProxyPort}", NativeMethods.Bypass);

            if (config.GlobalSocks5Port == 0)
            {
                config.GlobalSocks5Port = NetworkUtil.GetAvailablePort(3000);
            }

            PrivoxyManager.Exit();
            PrivoxyManager.Start(config.SystemProxyPort, config.GlobalSocks5Port);

            if (config.RemoteServer != null)
            {
                SSManager.Exit();
                SSManager.Start(config.RemoteServer, config.GlobalSocks5Port);
            }
        }
        private void TransmitDisable()
        {
            NativeMethods.DisableProxy();
            PrivoxyManager.Exit();
            SSManager.Exit();
        }

        // actoins ======================================================================================================
        // a functional interface
        public void AddServerByScanQRCode()
        {
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
                    SSManager.Exit();
                    SSManager.Start(serverProfile, App.GlobalConfig.GlobalSocks5Port);
                }
            }

            App.GlobalConfig.RemoteServer = serverProfile;
            OnPropertyChanged("RemoteServerName");
        }


        /** Commands ======================================================================================================
         */
        public RelayCommand CommandSwitchContent => new RelayCommand(switchContent);
        private void switchContent(object newTitle)
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
        public RelayCommand CommandShowSetting => new RelayCommand(showSetting);
        private void showSetting(object parameter)
        {
            new View.DialogSetting().ShowDialog();
        }

        // show about
        public RelayCommand CommandShowAbout => new RelayCommand(showAbout);
        private void showAbout(object parameter)
        {
            new View.DialogAbout().ShowDialog();
        }

        // exit
        public RelayCommand CommandExit => new RelayCommand(exitApp);
        private void exitApp(object parameter)
        {
            App.CloseMainWindow();
        }
    }
}
