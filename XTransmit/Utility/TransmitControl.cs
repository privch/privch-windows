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

        public static void ChangeTransmitServer(ServerProfile serverProfile)
        {
            if (App.GlobalConfig.RemoteServer == null || !App.GlobalConfig.RemoteServer.Equals(serverProfile))
            {
                ServerManager.Stop(App.GlobalConfig.RemoteServer);
                ServerManager.Start(serverProfile, App.GlobalConfig.GlobalSocks5Port);

                App.GlobalConfig.RemoteServer = serverProfile;
            }
        }
    }
}
