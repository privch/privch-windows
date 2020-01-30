using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XTransmit.Model.Server;
using XTransmit.Utility;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    public class ServerConfigVModel : BaseViewModel
    {
        public ServerProfile ServerEdit { get; }

        public List<ItemView> ServerInfo { get; private set; }

        public bool IsProcessing
        {
            get => processing_fetch_info
                || processing_check_response_time
                || processing_check_ping;
        }

        // post action
        private readonly Action<bool> actionComplete;

        // language
        private static readonly string sr_created = (string)Application.Current.FindResource("_created");
        private static readonly string sr_respond_time = (string)Application.Current.FindResource("respond_time");
        private static readonly string sr_ping = (string)Application.Current.FindResource("_ping");

        private static readonly string sr_title = (string)Application.Current.FindResource("dialog_server_title");
        private static readonly string sr_not_availabe = (string)Application.Current.FindResource("not_availabe");
        private static readonly string sr_invalid_ip = (string)Application.Current.FindResource("invalid_ip");
        private static readonly string sr_invalid_port = (string)Application.Current.FindResource("invalid_port");

        public ServerConfigVModel(ServerProfile serverProfile, Action<bool> actionComplete)
        {
            ServerEdit = serverProfile;
            ServerInfo = UpdateServerInfo();

            this.actionComplete = actionComplete;
        }

        private List<ItemView> UpdateServerInfo()
        {
            // TODO - a little overhead
            return new List<ItemView>()
            {
                new ItemView{Label = sr_created, Text = ServerEdit.TimeCreated},
                new ItemView{Label = sr_respond_time, Text = ServerEdit.ResponseTime},
                new ItemView{Label = sr_ping, Text = ServerEdit.Ping.ToString(CultureInfo.InvariantCulture)},

                new ItemView{Label = "Country", Text = ServerEdit.IPData?.Country ?? sr_not_availabe},
                new ItemView{Label = "Region", Text = ServerEdit.IPData?.Region ?? sr_not_availabe},
                new ItemView{Label = "City", Text = ServerEdit.IPData?.City ?? sr_not_availabe},
                new ItemView{Label = "Location", Text = ServerEdit.IPData?.Location ?? sr_not_availabe},
                new ItemView{Label = "Organization", Text = ServerEdit.IPData?.Organization ?? sr_not_availabe},
                new ItemView{Label = "Postal", Text = ServerEdit.IPData?.Postal ?? sr_not_availabe},
                new ItemView{Label = "Hostname", Text = ServerEdit.IPData?.Hostname ?? sr_not_availabe},
                new ItemView{Label = "Timezone", Text = ServerEdit.IPData.Timezone ?? sr_not_availabe},
            };
        }


        /** Commands ======================================================================================================
         */
        private volatile bool processing_fetch_info = false; // also use to cancel task
        private volatile bool processing_check_response_time = false; // also use to cancel task
        private volatile bool processing_check_ping = false;  // also use to cancel task

        // fetch ipinfo
        public RelayCommand CommandFetchInfo => new RelayCommand(FetchServerInfo, CanFetchInfo);
        private bool CanFetchInfo(object parameter) => !processing_fetch_info;
        private async void FetchServerInfo(object parameter)
        {
            processing_fetch_info = true;
            OnPropertyChanged(nameof(IsProcessing));

            await Task.Run(() =>
            {
                ServerEdit.UpdateIPInfo(true);
            }).ConfigureAwait(true);

            // update the data to the view
            ServerInfo = UpdateServerInfo();
            OnPropertyChanged(nameof(ServerInfo));

            processing_fetch_info = false;
            OnPropertyChanged(nameof(IsProcessing));
            CommandManager.InvalidateRequerySuggested();
        }

        // check response time
        public RelayCommand CommandCheckResponseTime => new RelayCommand(CheckResponseTime, CanCheckResponseTime);
        private bool CanCheckResponseTime(object parameter) => !processing_check_response_time;
        private async void CheckResponseTime(object parameter)
        {
            processing_check_response_time = true;
            OnPropertyChanged(nameof(IsProcessing));

            await Task.Run(() =>
            {
                int listen = NetworkUtil.GetAvailablePort(10000);
                if (listen > 0)
                {
                    ServerManager.Start(ServerEdit, listen);
                    ServerEdit.UpdateResponseTime();
                    ServerManager.Stop(ServerEdit);
                }
            }).ConfigureAwait(true);

            // update the data to the view
            ServerInfo = UpdateServerInfo();
            OnPropertyChanged(nameof(ServerInfo));

            processing_check_response_time = false;
            OnPropertyChanged(nameof(IsProcessing));
            CommandManager.InvalidateRequerySuggested();
        }

        // check ping 
        public RelayCommand CommandCheckPing => new RelayCommand(CheckPing, CanCheckPing);
        private bool CanCheckPing(object parameter) => !processing_check_ping;
        private async void CheckPing(object parameter)
        {
            processing_check_ping = true;
            OnPropertyChanged(nameof(IsProcessing));

            await Task.Run(() =>
            {
                ServerEdit.UpdatePing();
            }).ConfigureAwait(true);

            // update the data to the view
            ServerInfo = UpdateServerInfo();
            OnPropertyChanged(nameof(ServerInfo));

            processing_check_ping = false;
            OnPropertyChanged(nameof(IsProcessing));
            CommandManager.InvalidateRequerySuggested();
        }

        // ok
        public RelayCommand CommandCloseOK => new RelayCommand(CloseOK, CanCloseOK);
        private bool CanCloseOK(object parameter) => !IsProcessing;
        private void CloseOK(object parameter)
        {
            Window window = (Window)parameter;

            /** check values
             */
            Match matchIP = Regex.Match(ServerEdit.HostIP, RegexHelper.IPv4AddressRegex);
            if (!matchIP.Success)
            {
                new View.DialogPrompt(sr_title, sr_invalid_ip).ShowDialog();
                return;
            }

            if (ServerEdit.HostPort < 1 || ServerEdit.HostPort > 65535)
            {
                new View.DialogPrompt(sr_title, sr_invalid_port).ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(ServerEdit.FriendlyName))
            {
                ServerEdit.SetFriendlyNameDefault();
            }

            actionComplete?.Invoke(true);
            window.Close();
        }

        // cancel
        public RelayCommand CommandCloseCancel => new RelayCommand(CloseCancel);
        private void CloseCancel(object parameter)
        {
            if (parameter is Window window)
            {
                actionComplete?.Invoke(false);
                window.Close();
            }
        }
    }
}
