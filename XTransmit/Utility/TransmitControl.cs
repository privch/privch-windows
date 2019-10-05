using System.Collections.Generic;
using XTransmit.Model;
using XTransmit.Model.Server;

namespace XTransmit.Utility
{
    /**
     * Updated: 2019-10-04
     */
    static class TransmitControl
    {
        public static void EnableTransmit()
        {
            Config config = App.GlobalConfig;
            if (config.RemoteServer == null)
            {
                return;
            }

            if (config.SystemProxyPort == 0)
            {
                config.SystemProxyPort = NetworkUtil.GetAvailablePort(2000);
            }
            else
            {
                List<int> portInUse = NetworkUtil.GetPortInUse(2000);
                if (portInUse.Contains(config.SystemProxyPort))
                {
                    config.SystemProxyPort = NetworkUtil.GetAvailablePort(2000, portInUse);
                }
            }
            NativeMethods.EnableProxy($"127.0.0.1:{config.SystemProxyPort}", NativeMethods.Bypass);

            if (config.GlobalSocks5Port == 0)
            {
                config.GlobalSocks5Port = NetworkUtil.GetAvailablePort(3000);
            }
            else
            {
                List<int> portInUse = NetworkUtil.GetPortInUse(3000);
                if (portInUse.Contains(config.GlobalSocks5Port))
                {
                    config.GlobalSocks5Port = NetworkUtil.GetAvailablePort(3000, portInUse);
                }
            }

            PrivoxyManager.Start(config.SystemProxyPort, config.GlobalSocks5Port);
            if (config.RemoteServer != null)
            {
                ServerManager.Start(config.RemoteServer, config.GlobalSocks5Port);
            }

            App.GlobalConfig.IsTransmitEnabled = true;
        }

        public static void DisableTransmit()
        {
            NativeMethods.DisableProxy();
            PrivoxyManager.Stop();
            ServerManager.Stop(App.GlobalConfig.RemoteServer);

            App.GlobalConfig.IsTransmitEnabled = false;
        }

        public static void ChangeTransmitServer(ServerProfile serverProfile)
        {
            if (App.GlobalConfig.RemoteServer == null || !App.GlobalConfig.RemoteServer.Equals(serverProfile))
            {
                if (App.GlobalConfig.IsTransmitEnabled)
                {
                    ServerManager.Stop(App.GlobalConfig.RemoteServer);
                    ServerManager.Start(serverProfile, App.GlobalConfig.GlobalSocks5Port);
                }

                App.GlobalConfig.RemoteServer = serverProfile;
            }
        }
    }
}
