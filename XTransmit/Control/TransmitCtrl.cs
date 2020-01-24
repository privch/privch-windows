using System.Collections.Generic;
using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.Utility;

namespace XTransmit.Control
{
    internal static class TransmitCtrl
    {
        public static bool StartServer()
        {
            Config config = App.GlobalConfig;
            List<int> portInUse = NetworkUtil.GetPortInUse(2000);

            // proxy port
            if (config.SystemProxyPort == 0 || portInUse.Contains(config.SystemProxyPort))
            {
                config.SystemProxyPort = NetworkUtil.GetAvailablePort(2000, portInUse);
                portInUse.Add(config.SystemProxyPort);
            }
            else
            {
                portInUse.Add(config.SystemProxyPort);
            }

            // shadowsocks port
            if (config.GlobalSocks5Port == 0 || portInUse.Contains(config.GlobalSocks5Port))
            {
                config.GlobalSocks5Port = NetworkUtil.GetAvailablePort(2000, portInUse);
                portInUse.Add(config.GlobalSocks5Port);
            }
            else
            {
                portInUse.Add(config.GlobalSocks5Port);
            }

            if (!PrivoxyManager.Start(config.SystemProxyPort, config.GlobalSocks5Port))
            {
                return false;
            }

            if (config.RemoteServer != null)
            {
                return ServerManager.Start(config.RemoteServer, config.GlobalSocks5Port);
            }

            return true;
        }

        public static void StopServer()
        {
            PrivoxyManager.Stop();
            ServerManager.Stop(App.GlobalConfig.RemoteServer);
        }

        public static void EnableTransmit(bool enable)
        {
            if (enable)
            {
                if (NativeMethods.EnableProxy($"127.0.0.1:{App.GlobalConfig.SystemProxyPort}", NativeMethods.Bypass) != 0)
                {
                    App.GlobalConfig.IsTransmitEnabled = true;
                }
            }
            else
            {
                if (NativeMethods.DisableProxy() != 0)
                {
                    App.GlobalConfig.IsTransmitEnabled = false;
                }
            }

            InterfaceCtrl.UpdateHomeTransmitStatue();
            App.NotifyIcon.SwitchIcon(App.GlobalConfig.IsTransmitEnabled);
        }

        public static void ChangeTransmitServer(ServerProfile serverProfile)
        {
            if (App.GlobalConfig.RemoteServer == null || !App.GlobalConfig.RemoteServer.Equals(serverProfile))
            {
                ServerManager.Stop(App.GlobalConfig.RemoteServer);
                ServerManager.Start(serverProfile, App.GlobalConfig.GlobalSocks5Port);

                App.GlobalConfig.RemoteServer = serverProfile;
                InterfaceCtrl.UpdateHomeTransmitStatue();
            }
        }
    }
}
