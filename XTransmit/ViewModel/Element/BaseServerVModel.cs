using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace XTransmit.ViewModel.Element
{
    internal abstract class BaseServerVModel : INotifyPropertyChanged
    {
        protected IEnumerable<IServer> Servers;

        // status, also use to cancel task
        protected volatile bool processing_fetch_info = false;
        protected volatile bool processing_check_response = false;
        protected volatile bool processing_check_ping = false;

        // language
        private static readonly string sr_yes = (string)Application.Current.FindResource("_yes");
        private static readonly string sr_no = (string)Application.Current.FindResource("_no");
        private static readonly string sr_cancel = (string)Application.Current.FindResource("_cancel");

        private static readonly string sr_task_fetch_info = (string)Application.Current.FindResource("task_fetch_info");
        private static readonly string sr_task_ping_server = (string)Application.Current.FindResource("task_ping_server");
        private static readonly string sr_task_check_response_time = (string)Application.Current.FindResource("task_check_response_time");

        private static readonly string sr_fetch_ask_focus_title = (string)Application.Current.FindResource("server_fetch_info");
        private static readonly string sr_fetch_ask_focus_message = (string)Application.Current.FindResource("server_fetch_ask_force");
        

        public bool CanEditList(object parameter)
        {
            return !processing_fetch_info && !processing_check_response && !processing_check_ping;
        }

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
                IEnumerator<IServer> enumerator = Servers.GetEnumerator();
                int count = Servers.Count();
                int complete = 0;

                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    // cancel task
                    if (processing_fetch_info == false)
                    {
                        break;
                    }

                    enumerator.Current.UpdateIPInfo((bool)force);
                    task.Progress100 = ++complete * 100 / count;
                }

                enumerator.Dispose();
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
            return processing_fetch_info == false && parameter is IServer;
        }

        private async void FetchInfoSelected(object parameter)
        {
            IServer server = (IServer)parameter;

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


        #region Command-CheckResponse
        // check response time for all servers
        public RelayCommand CommandCheckResponseAll => new RelayCommand(CheckResponseAll, CanCheckResponseAll);

        private bool CanCheckResponseAll(object parameter) => !processing_check_response;

        private async void CheckResponseAll(object parameter)
        {
            // add task
            processing_check_response = true;
            TaskView task = new TaskView
            {
                Name = sr_task_check_response_time,
                StopAction = () => { processing_check_response = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            // run
            await Task.Run(() =>
            {
                IEnumerator<IServer> enumerator = Servers.GetEnumerator();
                int count = Servers.Count();
                int complete = 0;

                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    // cancel task
                    if (processing_check_response == false)
                    {
                        break;
                    }

                    IServer server = enumerator.Current;
                    if (ServerManager.ServerProcessMap.ContainsKey(server.GetID()))
                    {
                        server.UpdateResponse();
                    }
                    else
                    {
                        int listen = NetworkUtil.GetAvailablePort(10000);
                        if (listen > 0)
                        {
                            ServerManager.Start(server, listen);
                            server.UpdateResponse();
                            ServerManager.Stop(server);
                        }
                    }

                    task.Progress100 = ++complete * 100 / count;
                }
            }).ConfigureAwait(true);

            // done
            processing_check_response = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }

        // check response for selected servers
        public RelayCommand CommandCheckResponseSelected => new RelayCommand(CheckResponseSelected, CanCheckResponseSelected);

        private bool CanCheckResponseSelected(object parameter)
        {
            return !processing_check_response && parameter is IServer;
        }

        private async void CheckResponseSelected(object parameter)
        {
            IServer server = (IServer)parameter;

            // add task
            processing_check_response = true;
            TaskView task = new TaskView
            {
                Name = sr_task_check_response_time,
                StopAction = null
            };
            InterfaceCtrl.AddHomeTask(task);

            // run
            await Task.Run(() =>
            {
                if (ServerManager.ServerProcessMap.ContainsKey(server.GetID()))
                {
                    server.UpdateResponse();
                }
                else
                {
                    int listen = NetworkUtil.GetAvailablePort(10000);
                    if (listen > 0)
                    {
                        ServerManager.Start(server, listen);
                        server.UpdateResponse();
                        ServerManager.Stop(server);
                    }
                }

                task.Progress100 = 100;
            }).ConfigureAwait(true);

            // done
            processing_check_response = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion


        #region Command-CheckPing
        // check ping for all servers
        public RelayCommand CommandCheckPingAll => new RelayCommand(CheckPingAll, CanCheckPingAll);

        private bool CanCheckPingAll(object parameter) => !processing_check_ping;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async void CheckPingAll(object parameter)
        {
            // add task
            processing_check_ping = true;
            TaskView task = new TaskView
            {
                Name = sr_task_ping_server,
                StopAction = () => { processing_check_ping = false; }
            };
            InterfaceCtrl.AddHomeTask(task);

            // run
            IEnumerator<IServer> enumerator = Servers.GetEnumerator();
            int timeout = ConfigManager.Global.PingTimeout;

            using (Ping pingSender = new Ping())
            {
                int count = Servers.Count();
                int complete = 0;

                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    // isPingInProcess is also use to cancel task
                    if (processing_check_ping == false)
                    {
                        break;
                    }

                    IServer server = enumerator.Current;
                    try
                    {
                        PingReply reply = await pingSender.SendPingAsync(server.HostAddress, timeout).ConfigureAwait(true);
                        server.PingDelay = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
                    }
                    catch
                    {
                        server.PingDelay = -1;
                    }

                    task.Progress100 = ++complete * 100 / count;
                }
            }

            // done
            processing_check_ping = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }

        // check pin for selected server
        public RelayCommand CommandCheckPingSelected => new RelayCommand(CheckPingSelected, CanCheckPingSelected);

        private bool CanCheckPingSelected(object parameter)
        {
            return !processing_check_ping && parameter is IServer;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async void CheckPingSelected(object parameter)
        {
            IServer server = (IServer)parameter;

            // add task
            processing_check_ping = true;
            TaskView task = new TaskView
            {
                Name = sr_task_ping_server,
                StopAction = null
            };
            InterfaceCtrl.AddHomeTask(task);

            // run
            await Task.Run(() =>
            {
                server.UpdatePing();
                task.Progress100 = 100;
            }).ConfigureAwait(true);

            // done
            processing_check_ping = false;
            InterfaceCtrl.RemoveHomeTask(task);
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged
    }
}
