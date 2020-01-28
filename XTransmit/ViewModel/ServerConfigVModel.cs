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

        public List<ItemView> ServerIPInfo { get; private set; }

        private bool vIsFetching = false;
        public bool IsFetching
        {
            get => vIsFetching;
            private set
            {
                vIsFetching = value;
                OnPropertyChanged(nameof(IsFetching));
            }
        }

        // post action
        private readonly Action<bool> actionComplete;

        // language
        private static readonly string sr_title = (string)Application.Current.FindResource("dialog_server_title");
        private static readonly string sr_not_availabe = (string)Application.Current.FindResource("not_availabe");
        private static readonly string sr_invalid_ip = (string)Application.Current.FindResource("invalid_ip");
        private static readonly string sr_invalid_port = (string)Application.Current.FindResource("invalid_port");

        public ServerConfigVModel(ServerProfile serverProfile, Action<bool> actionComplete)
        {
            ServerEdit = serverProfile;
            ServerIPInfo = UpdateInfo();

            this.actionComplete = actionComplete;
        }

        private List<ItemView> UpdateInfo()
        {
            return new List<ItemView>()
            {
                new ItemView{Label = "Created", Text = ServerEdit.TimeCreated ?? sr_not_availabe},
                new ItemView{Label = "Last Ping (ms)", Text = ServerEdit.Ping.ToString(CultureInfo.InvariantCulture)},

                new ItemView{Label = "Country", Text = ServerEdit.IPData?.Country ?? sr_not_availabe},
                new ItemView{Label = "Region", Text = ServerEdit.IPData?.Region ?? sr_not_availabe},
                new ItemView{Label = "City", Text = ServerEdit.IPData?.City ?? sr_not_availabe},
                new ItemView{Label = "Location", Text = ServerEdit.IPData?.Location ?? sr_not_availabe},
                new ItemView{Label = "Org", Text = ServerEdit.IPData?.Organization ?? sr_not_availabe},
                new ItemView{Label = "Postal", Text = ServerEdit.IPData?.Postal ?? sr_not_availabe},
                //new ItemInfo{Label = "Host Name", Text = ServerInfoData.vServerProfile.IPData?.hostname ?? sr_not_availabe},
            };
        }


        /** Commands ======================================================================================================
         */
        private bool IsNotFetching(object parameter) => !IsFetching;

        // fetch ipinfo
        public RelayCommand CommandFetchData => new RelayCommand(FetchDataAsync, IsNotFetching);
        private async void FetchDataAsync(object parameter)
        {
            IsFetching = true;

            await Task.Run(() =>
            {
                ServerEdit.UpdateIPInfo(true);
            }).ConfigureAwait(true);

            ServerIPInfo = UpdateInfo();
            OnPropertyChanged(nameof(ServerIPInfo));

            IsFetching = false;
            CommandManager.InvalidateRequerySuggested();
        }

        // ok
        public RelayCommand CommandCloseOK => new RelayCommand(CloseOK, IsNotFetching);
        private void CloseOK(object parameter)
        {
            Window window = (Window)parameter;

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
