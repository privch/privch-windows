using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Privch.Control;
using Privch.Model;

namespace Privch.ViewModel.Element
{
    internal abstract class BaseServerVModel : INotifyPropertyChanged
    {
        protected IEnumerable<BaseServer> Servers;

        // status, also use to cancel task
        protected volatile bool processing_check_ping;

        // language
        private static readonly string sr_task_ping_server = (string)Application.Current.FindResource("task_check_ping");


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
            int count = Servers.Count();
            int timeout = SettingManager.Configuration.TimeoutPing;

            using (Ping pingSender = new Ping())
            {
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

                enumerator.Dispose();
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
            return !processing_check_ping && parameter is System.Collections.IList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async void CheckPingSelected(object parameter)
        {
            // add task
            processing_check_ping = true;
            TaskView task = new TaskView
            {
                Name = sr_task_ping_server,
                StopAction = null
            };
            InterfaceCtrl.AddHomeTask(task);

            // check selection items;
            IEnumerator<BaseServer> enumerator = (parameter as System.Collections.IList).Cast<BaseServer>().GetEnumerator();
            List<BaseServer> selection = new List<BaseServer>();
            while (enumerator.MoveNext())
            {
                selection.Add(enumerator.Current);
            }

            for (int i = 0; i < selection.Count; ++i)
            {
                if (!processing_check_ping)
                {
                    break;
                }

                await Task.Run(() => selection[i].UpdatePingDelay()).ConfigureAwait(true);
                task.Progress100 = i * 100 / selection.Count;
            }

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
