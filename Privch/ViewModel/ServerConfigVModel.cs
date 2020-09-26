using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Privch.Model;
using Privch.Utility;
using Privch.ViewModel.Element;

namespace Privch.ViewModel
{
    public class ServerConfigVModel : BaseViewModel
    {
        #region Properties
        public BaseServer ServerBase { get; }
        public UserControl ContentDisplay { get; }

        public bool IsProcessing
        {
            get => processing_check_ping;
        }
        #endregion


        private readonly Action<bool> actionComplete;

        // status. also use to cancel task
        private volatile bool processing_check_ping;

        // language
        private readonly string promptTitle;
        private static readonly string sr_invalid_ip = (string)Application.Current.FindResource("invalid_ip");
        private static readonly string sr_invalid_port = (string)Application.Current.FindResource("invalid_port");

        public ServerConfigVModel(BaseServer server, Action<bool> actionComplete)
        {
            ServerBase = server;

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


        /** Commands ======================================================================================================
         */
        // check ping 
        public RelayCommand CommandCheckPing => new RelayCommand(CheckPing, CanCheckPing);
        private bool CanCheckPing(object parameter) => !processing_check_ping;
        private async void CheckPing(object parameter)
        {
            processing_check_ping = true;
            OnPropertyChanged(nameof(IsProcessing));

            await Task.Run(() =>
            {
                ServerBase.UpdatePingDelay();
            }).ConfigureAwait(true);

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
            if (ServerBase is Model.SS.Shadowsocks)
            {
                Match matchIP = Regex.Match(ServerBase.HostAddress, RegexHelper.IPv4AddressRegex);
                if (!matchIP.Success)
                {
                    new View.DialogPrompt(promptTitle, sr_invalid_ip).ShowDialog();
                    return;
                }
            }

            if (ServerBase.HostPort < 1 || ServerBase.HostPort > 65535)
            {
                new View.DialogPrompt(promptTitle, sr_invalid_port).ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(ServerBase.FriendlyName))
            {
                ServerBase.SetFriendlyNameDefault();
            }

            actionComplete?.Invoke(true);
            window.Close();
        }

        // cancel
        public RelayCommand CommandCloseCancel => new RelayCommand(CloseCancel);
        private void CloseCancel(object parameter)
        {
            Window window = (Window)parameter;

            actionComplete?.Invoke(false);
            window.Close();
        }
    }
}
