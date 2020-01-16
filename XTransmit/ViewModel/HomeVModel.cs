using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    public class HomeVModel : BaseViewModel
    {
        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public string TransmitStatus => App.GlobalConfig.RemoteServer?.FriendlyName ?? sr_server_not_set;

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public bool IsTransmitControllable => !App.GlobalConfig.IsServerPoolEnabled;

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public bool IsTransmitEnabled
        {
            get => App.GlobalConfig.IsTransmitEnabled;
            set => App.EnableTransmit(value);
        }

        // progress
        public ProgressView Progress { get; private set; }
        public ObservableCollection<TaskView> TaskListOC { get; private set; }

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
            Progress = new ProgressView(0, false);
            TaskListOC = new ObservableCollection<TaskView>();

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

            // to trigge the control
            IsTransmitEnabled = App.GlobalConfig.IsTransmitEnabled;
            App.GlobalConfig.IsServerPoolEnabled = false;
        }

        /** methods ====================================================================================================== 
         */
        public string GetCurrentContent()
        {
            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.IsChecked);
            return contentTable.Title;
        }

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

        // Progress is indeterminated
        public void AddTask(TaskView task)
        {
            if (!TaskListOC.Contains(task))
            {
                // max value: 100
                TaskListOC.Add(task);

                Progress.Value += (100 - Progress.Value) >> 1;
                Progress.IsIndeterminate = true;

                OnPropertyChanged(nameof(Progress));
            }
        }
        public void RemoveTask(TaskView task)
        {
            if (TaskListOC.Contains(task))
            {
                TaskListOC.Remove(task);

                Progress.Value -= (100 - Progress.Value);
                Progress.IsIndeterminate = Progress.Value > 0;

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

        public RelayCommand CommandStopTask => new RelayCommand(StopTask);
        private void StopTask(object parameter)
        {
            if (parameter is string id)
            {
                TaskView taskView = TaskListOC.FirstOrDefault(task => task.Name == id);
                taskView?.StopAction?.Invoke();
            }
        }

        // open the xcurl
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
