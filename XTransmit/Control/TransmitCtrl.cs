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
            Config config = ConfigManager.Global;
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

            if (ConfigManager.RemoteServer != null)
            {
                return ServerManager.Start(ConfigManager.RemoteServer, config.GlobalSocks5Port);
            }

            return true;
        }

        public static void StopServer()
        {
            PrivoxyManager.Stop();
            ServerManager.Stop(ConfigManager.RemoteServer);
        }

        public static void EnableTransmit(bool enable)
        {
            if (enable)
            {
                if (NativeMethods.EnableProxy($"127.0.0.1:{ConfigManager.Global.SystemProxyPort}", NativeMethods.Bypass) != 0)
                {
                    ConfigManager.Global.IsTransmitEnabled = true;
                }
            }
            else
            {
                if (NativeMethods.DisableProxy() != 0)
                {
                    ConfigManager.Global.IsTransmitEnabled = false;
                }
            }
        }

        public static void ChangeTransmitServer(Shadowsocks serverProfile)
        {
            if (ConfigManager.RemoteServer == null || !ConfigManager.RemoteServer.IsServerEqual(serverProfile))
            {
                ServerManager.Stop(ConfigManager.RemoteServer);
                ServerManager.Start(serverProfile, ConfigManager.Global.GlobalSocks5Port);

                ConfigManager.RemoteServer = serverProfile;
                InterfaceCtrl.UpdateHomeTransmitStatue();
            }
        }
    }
}
