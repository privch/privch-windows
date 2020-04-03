using System.Collections.Generic;
using Privch.Model;
using Privch.Model.Setting;
using Privch.Utility;

namespace Privch.Control
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
            if (config.LocalSocks5Port == 0 || portInUse.Contains(config.LocalSocks5Port))
            {
                config.LocalSocks5Port = NetworkUtil.GetAvailablePort(2000, portInUse);
                portInUse.Add(config.LocalSocks5Port);
            }
            else
            {
                portInUse.Add(config.LocalSocks5Port);
            }

            if (!ProcPrivoxy.Start(config.SystemProxyPort, config.LocalSocks5Port))
            {
                return false;
            }

            if (SettingManager.RemoteServer != null)
            {
                return ServerManager.AddProcess(SettingManager.RemoteServer, config.LocalSocks5Port);
            }

            return true;
        }

        public static void StopServer()
        {
            ProcPrivoxy.Stop();
            ServerManager.RemoveProcess(SettingManager.RemoteServer);
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

        public static bool ChangeTransmitServer(BaseServer server)
        {
            if (SettingManager.RemoteServer == null || !SettingManager.RemoteServer.IsServerEqual(server))
            {
                ServerManager.RemoveProcess(SettingManager.RemoteServer);
                if (ServerManager.AddProcess(server, SettingManager.Configuration.LocalSocks5Port))
                {
                    SettingManager.RemoteServer = server;
                    return true;
                }
            }

            return false;
        }
    }
}
