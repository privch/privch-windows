using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XTransmit.Control;
using XTransmit.Model;

namespace XTransmit.ViewModel.Element
{
    internal abstract class BaseServerVModel : INotifyPropertyChanged
    {
        protected IEnumerable<BaseServer> Servers;

        // status, also use to cancel task
        protected volatile bool processing_check_ping = false;

        // language
        private static readonly string sr_task_ping_server = (string)Application.Current.FindResource("task_ping_server");


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
            IEnumerator<BaseServer> enumerator = Servers.GetEnumerator();
            int timeout = SettingManager.Configuration.TimeoutPing;

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

                    BaseServer server = enumerator.Current;
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
            return !processing_check_ping && parameter is BaseServer;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async void CheckPingSelected(object parameter)
        {
            BaseServer server = (BaseServer)parameter;

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
                server.UpdatePingDelay();
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
