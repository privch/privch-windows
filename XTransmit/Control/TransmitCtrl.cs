using System.Collections.Generic;
using XTransmit.Model;
using XTransmit.Model.Setting;
using XTransmit.Utility;

namespace XTransmit.Control
{
    internal static class TransmitCtrl
    {
        public static bool StartServer()
        {
            Config config = SettingManager.Configuration;
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

            if (!ProcPrivoxy.Start(config.SystemProxyPort, config.GlobalSocks5Port))
            {
                return false;
            }

            if (SettingManager.RemoteServer != null)
            {
                return ServerManager.Start(SettingManager.RemoteServer, config.GlobalSocks5Port);
            }

            return true;
        }

        public static void StopServer()
        {
            ProcPrivoxy.Stop();
            ServerManager.Stop(SettingManager.RemoteServer);
        }

        public static void EnableTransmit(bool enable)
        {
            if (enable)
            {
                if (NativeMethods.EnableProxy($"127.0.0.1:{SettingManager.Configuration.SystemProxyPort}", NativeMethods.Bypass) != 0)
                {
                    SettingManager.Configuration.IsTransmitEnabled = true;
                }
            }
            else
            {
                if (NativeMethods.DisableProxy() != 0)
                {
                    SettingManager.Configuration.IsTransmitEnabled = false;
                }
            }
        }

        public static void ChangeTransmitServer(BaseServer server)
        {
            if (SettingManager.RemoteServer == null || !SettingManager.RemoteServer.IsServerEqual(server))
            {
                ServerManager.Stop(SettingManager.RemoteServer);
                ServerManager.Start(server, SettingManager.Configuration.GlobalSocks5Port);

                SettingManager.RemoteServer = server;
                InterfaceCtrl.UpdateHomeTransmitStatue();
            }
        }
    }
}
