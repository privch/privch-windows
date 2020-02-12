using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XTransmit.Model;
using XTransmit.Utility;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    public class ServerConfigVModel : BaseViewModel
    {
        public List<ItemView> ServerInfo { get; private set; }

        public UserControl ContentDisplay { get; private set; }

        public bool IsProcessing
        {
            get => processing_fetch_info || processing_check_ping;
        }

        private readonly BaseServer serverBase;
        private readonly Action<bool> actionComplete;

        // status. also use to cancel task
        private volatile bool processing_fetch_info = false;
        private volatile bool processing_check_ping = false;

        // language
        private readonly string promptTitle;

        private static readonly string sr_modified = (string)Application.Current.FindResource("_modified");
        private static readonly string sr_respond_time = (string)Application.Current.FindResource("response_time");
        private static readonly string sr_ping = (string)Application.Current.FindResource("_ping");

        private static readonly string sr_not_availabe = (string)Application.Current.FindResource("not_availabe");
        private static readonly string sr_invalid_ip = (string)Application.Current.FindResource("invalid_ip");
        private static readonly string sr_invalid_port = (string)Application.Current.FindResource("invalid_port");

        public ServerConfigVModel(BaseServer server, Action<bool> actionComplete)
        {
            serverBase = server;
            ServerInfo = UpdateServerInfo();

            if (server is Model.SS.Shadowsocks)
            {
                ContentDisplay = new View.ServerConfigShadowsocks(server as Model.SS.Shadowsocks);
                promptTitle = (string)Application.Current.FindResource("dialog_server_shadowsocks");
            }
            else if (server is Model.V2Ray.V2RayVMess)
            {
                ContentDisplay = new View.ServerConfigV2Ray(server as Model.V2Ray.V2RayVMess);
                promptTitle = (string)Application.Current.FindResource("dialog_server_v2ray");
            }
            else
            {
                throw new ArgumentException(System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            this.actionComplete = actionComplete;
        }

        private List<ItemView> UpdateServerInfo()
        {
            // a bit overhead
            return new List<ItemView>()
            {
                new ItemView{Label = sr_modified, Text = serverBase.Modified},
                new ItemView{Label = sr_respond_time, Text = serverBase.ResponseTime},
                new ItemView{Label = sr_ping, Text = serverBase.PingDelay.ToString(CultureInfo.InvariantCulture)},

                new ItemView{Label = "Country", Text = serverBase.IPInfo?.Country ?? sr_not_availabe},
                new ItemView{Label = "Region", Text = serverBase.IPInfo?.Region ?? sr_not_availabe},
                new ItemView{Label = "City", Text = serverBase.IPInfo?.City ?? sr_not_availabe},
                new ItemView{Label = "Location", Text = serverBase.IPInfo?.Location ?? sr_not_availabe},
                new ItemView{Label = "Organization", Text = serverBase.IPInfo?.Organization ?? sr_not_availabe},
                new ItemView{Label = "Postal", Text = serverBase.IPInfo?.Postal ?? sr_not_availabe},
                new ItemView{Label = "Hostname", Text = serverBase.IPInfo?.Hostname ?? sr_not_availabe},
                new ItemView{Label = "Timezone", Text = serverBase.IPInfo?.Timezone ?? sr_not_availabe},
            };
        }


        /** Commands ======================================================================================================
         */
        // fetch ipinfo
        public RelayCommand CommandFetchInfo => new RelayCommand(FetchServerInfo, CanFetchInfo);
        // TODO - Next
        private bool CanFetchInfo(object parameter) => !processing_fetch_info && serverBase is Model.SS.Shadowsocks;
        private async void FetchServerInfo(object parameter)
        {
            processing_fetch_info = true;
            OnPropertyChanged(nameof(IsProcessing));

            await Task.Run(() =>
            {
                serverBase.UpdateIPInfo(true);
            }).ConfigureAwait(true);

            // update the data to the view
            ServerInfo = UpdateServerInfo();
            OnPropertyChanged(nameof(ServerInfo));

            processing_fetch_info = false;
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
                serverBase.UpdatePingDelay();
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
            if (serverBase is Model.SS.Shadowsocks)
            {
                Match matchIP = Regex.Match(serverBase.HostAddress, RegexHelper.IPv4AddressRegex);
                if (!matchIP.Success)
                {
                    new View.DialogPrompt(promptTitle, sr_invalid_ip).ShowDialog();
                    return;
                }
            }

            if (serverBase.HostPort < 1 || serverBase.HostPort > 65535)
            {
                new View.DialogPrompt(promptTitle, sr_invalid_port).ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(serverBase.FriendlyName))
            {
                serverBase.SetFriendlyNameDefault();
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
