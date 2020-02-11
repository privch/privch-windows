using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XTransmit.Control;
using XTransmit.Model;
using XTransmit.Utility;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    public class HomeVModel : BaseViewModel
    {
        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public string TransmitStatus => SettingManager.RemoteServer?.FriendlyName ?? sr_server_not_set;

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public bool IsTransmitControllable => !SettingManager.IsServerPoolEnabled;

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public bool IsTransmitEnabled
        {
            get => SettingManager.Configuration.IsTransmitEnabled;
            set
            {
                TransmitCtrl.EnableTransmit(value);

                // update interface
                InterfaceCtrl.UpdateHomeTransmitStatue();
                InterfaceCtrl.NotifyIcon.UpdateIcon();
            }
        }

        // progress
        public ProgressView Progress { get; private set; }
        public ObservableCollection<TaskView> TaskListOC { get; private set; }

        // tables
        public UserControl ContentDisplay { get; private set; }
        public List<ContentTable> ContentList { get; }

        // language
        private static readonly string sr_server_not_set = (string)Application.Current.FindResource("home_server_not_set");
        private static readonly string sr_server_not_found = (string)Application.Current.FindResource("add_server_not_found");
        private static readonly string sr_shadowsocks_title = (string)Application.Current.FindResource("shadowsocks_title");
        private static readonly string sr_v2ray_title = (string)Application.Current.FindResource("v2ray_title");
        private static readonly string sr_network_title = (string)Application.Current.FindResource("netrowk_title");
        private static readonly string sr_task_running = (string)Application.Current.FindResource("home_x_task_running");
        private static readonly string sr_cant_add_server = (string)Application.Current.FindResource("home_cant_add_server");

        public HomeVModel()
        {
            // init progress
            Progress = new ProgressView(0, false, null);
            TaskListOC = new ObservableCollection<TaskView>();

            // init content list and display
            ContentList = new List<ContentTable>
            {
                new ContentTable(sr_shadowsocks_title, new View.ContentShadowsocks()),
                new ContentTable(sr_v2ray_title, new View.ContentV2Ray()),
                new ContentTable(sr_network_title, new View.ContentNetwork()),
            };

            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.Title == SettingManager.Appearance.HomeContentDisplay);
            if (contentTable == null)
            {
                contentTable = ContentList[0];
            }

            contentTable.IsChecked = true;
            ContentDisplay = contentTable.Content;
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

        public void UpdateTransmitLock()
        {
            OnPropertyChanged(nameof(IsTransmitControllable));
        }

        //ProgressBar is showing in indeterminate mode
        public void AddTask(TaskView task)
        {
            if (!TaskListOC.Contains(task))
            {
                // max value: 100
                TaskListOC.Add(task);

                Progress.Value += (100 - Progress.Value) >> 1;
                Progress.IsIndeterminate = true;
                Progress.Description = $"{TaskListOC.Count} {sr_task_running}";

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
                Progress.Description = $"{TaskListOC.Count} {sr_task_running}";

                OnPropertyChanged(nameof(Progress));
            }
        }

        public void AddServerByScanQRCode()
        {
            ZXing.Result result = QRCode.DecodeScreen();
            if (result == null || string.IsNullOrWhiteSpace(result.Text))
            {
                InterfaceCtrl.ShowHomeNotify(sr_server_not_found);
                InterfaceCtrl.NotifyIcon.ShowMessage(sr_server_not_found);
                return;
            }

            if (result.Text.StartsWith("ss://", StringComparison.OrdinalIgnoreCase))
            {
                ContentTable contantTable = ContentList.FirstOrDefault(item => item.Title == sr_shadowsocks_title);
                ContentShadowsocksVModel contentShadowsocks = (ContentShadowsocksVModel)contantTable.Content.DataContext;
                if (contentShadowsocks.CanEditList(null))
                {
                    contentShadowsocks.CommandAddServerQRCode.Execute(result.Text);
                }
                else
                {
                    InterfaceCtrl.NotifyIcon.ShowMessage(sr_cant_add_server);
                }
            }
            else if (result.Text.StartsWith("vmess://", StringComparison.OrdinalIgnoreCase))
            {
                ContentTable contantTable = ContentList.FirstOrDefault(item => item.Title == sr_v2ray_title);
                ContentV2RayVModel contentV2Ray = (ContentV2RayVModel)contantTable.Content.DataContext;
                if (contentV2Ray.CanEditList(null))
                {
                    contentV2Ray.CommandAddServerQRCode.Execute(result.Text);
                }
                else
                {
                    InterfaceCtrl.NotifyIcon.ShowMessage(sr_cant_add_server);
                }
            }
        }

        // not used
        public void AddServerFromClipboard()
        {
            ContentTable contantTable = ContentList.FirstOrDefault(item => item.Title == sr_shadowsocks_title);
            ContentShadowsocksVModel contentShadowsocks = (ContentShadowsocksVModel)contantTable.Content.DataContext;
            if (contentShadowsocks.CanEditList(null))
            {
                contentShadowsocks.CommandAddServerClipboard.Execute(null);
            }
            else
            {
                InterfaceCtrl.NotifyIcon.ShowMessage(sr_cant_add_server);
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
            if (parameter is string name)
            {
                TaskView taskView = TaskListOC.FirstOrDefault(task => task.Name == name);
                taskView?.StopAction?.Invoke();
            }
        }

        // open the xcurl
        public RelayCommand CommandShowCurl => new RelayCommand(ShowCurl);
        private void ShowCurl(object parameter)
        {
            // save data first
            ContentTable contantTable = ContentList.FirstOrDefault(item => item.Title == sr_shadowsocks_title);
            ContentShadowsocksVModel contentShadowsocks = (ContentShadowsocksVModel)contantTable.Content.DataContext;
            contentShadowsocks.CommandSaveServer.Execute(null);

            // show curl
            View.WindowCurl windowCurl = Application.Current.Windows.OfType<View.WindowCurl>().FirstOrDefault();
            if (windowCurl == null)
            {
                windowCurl = new View.WindowCurl();
                windowCurl.Show();
            }
            else
            {
                windowCurl.Activate();
            }
        }

        // show setting
        public RelayCommand CommandShowSetting => new RelayCommand(ShowSetting);
        private void ShowSetting(object parameter)
        {
            InterfaceCtrl.ShowSetting();
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
