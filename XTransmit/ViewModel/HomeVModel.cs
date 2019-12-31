using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    public class HomeVModel : BaseViewModel
    {
        public static bool IsTransmitEnabled
        {
            get => App.GlobalConfig.IsTransmitEnabled;
            set => App.EnableTransmit(value);
        }

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public string TransmitStatus => App.GlobalConfig.RemoteServer?.FriendlyName ?? sr_server_not_set;

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public bool IsTransmitControllable => !App.GlobalConfig.IsServerPoolEnabled;

        // progress
        public ProgressInfo Progress { get; private set; }
        private readonly Dictionary<string, int> ProgressList;

        // table
        public UserControl ContentDisplay { get; private set; }
        public List<ContentTable> ContentList { get; private set; }

        // curl window
        private Window windowCurl = null;

        private static readonly string sr_server_not_set = (string)Application.Current.FindResource("home_server_not_set");
        private static readonly string sr_server_title = (string)Application.Current.FindResource("server_title");
        private static readonly string sr_network_title = (string)Application.Current.FindResource("netrowk_title");


        public HomeVModel()
        {
            // init progress
            Progress = new ProgressInfo(0, false);
            ProgressList = new Dictionary<string, int>();

            // init content list and display
            ContentList = new List<ContentTable>
            {
                new ContentTable(sr_server_title, new View.ContentServer()),
                new ContentTable(sr_network_title, new View.ContentNetwork()),
            };

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

        /** actoins ====================================================================================================== 
         */
        public void UpdateTransmitStatus()
        {
            OnPropertyChanged(nameof(IsTransmitEnabled));
            OnPropertyChanged(nameof(TransmitStatus));
        }

        public void UpdateLockTransmit()
        {
            OnPropertyChanged(nameof(IsTransmitControllable));
        }

        public void AddServerByScanQRCode()
        {
            // TODO - Take care of the ContentTables list order
            ContentServerVModel serverViewModel = (ContentServerVModel)ContentList[0].Content.DataContext;
            serverViewModel.CommandAddServerQRCode.Execute(null);
        }

        // Progress is indeterminated, This mothod increase/decrease the progress value.
        // TODO - Improve progress list
        public void AddProgress(string id)
        {
            if (!ProgressList.ContainsKey(id))
            {
                // max value: 100
                ProgressList.Add(id, 50);

                int newValue = 0;
                foreach(int value in ProgressList.Values)
                {
                    newValue += (100 - newValue) >> 1;
                }

                Progress.Value = newValue;
                Progress.IsIndeterminate = true;

                OnPropertyChanged(nameof(Progress));
            }
        }
        public void RemoveProgress(string id)
        {
            if (ProgressList.ContainsKey(id))
            {
                ProgressList.Remove(id);

                int newValue = 0;
                foreach (int value in ProgressList.Values)
                {
                    newValue += (100 - newValue) >> 1;
                }

                Progress.Value = newValue;
                if (Progress.Value == 0) Progress.IsIndeterminate = false;
                else Progress.IsIndeterminate = true;

                OnPropertyChanged(nameof(Progress));
            }
        }


        /** Commands ======================================================================================================
         */
        public RelayCommand CommandSelectContent => new RelayCommand(SelectContent);
        private void SelectContent(object newTitle)
        {
            if (newTitle is string title)
            {
                UserControl content = ContentList.FirstOrDefault(x => x.Title == title).Content;
                if (ContentDisplay != content)
                {
                    ContentDisplay = content;
                    OnPropertyChanged(nameof(ContentDisplay));
                }
            }
        }

        // open curl
        public RelayCommand CommandShowCurl => new RelayCommand(ShowCurl);
        private void ShowCurl(object parameter)
        {
            // TODO - Temporary
            ContentServerVModel serverViewModel = (ContentServerVModel)ContentList[0].Content.DataContext;
            serverViewModel.CommandSaveServer.Execute(null);

            if (windowCurl == null || !windowCurl.IsLoaded)
            {
                windowCurl = new View.WindowCurl();
            }

            if (windowCurl.WindowState == WindowState.Minimized)
            {
                windowCurl.WindowState = WindowState.Normal;
            }

            windowCurl.Show();
            windowCurl.Activate();
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
