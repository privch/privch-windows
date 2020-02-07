using XTransmit.Model;
using XTransmit.Model.Server;
using XTransmit.Utility;

namespace XTransmit.Control
{
    internal static class ServerPoolCtrl
    {
        public static void StartServerPool()
        {
            if (ServerManager.ServerList.Count < 1)
            {
                return;
            }

            foreach (Shadowsocks server in ServerManager.ServerList)
            {
                int listen = NetworkUtil.GetAvailablePort(2000);
                if (listen > 0)
                {
                    ServerManager.Start(server, listen);
                }
            }

            ConfigManager.IsServerPoolEnabled = true;
            InterfaceCtrl.UpdateTransmitLock();
        }

        public static void StopServerPool()
        {
            // exclude the transmit server
            if (ConfigManager.RemoteServer != null)
            {
                ServerManager.ServerList.Remove(ConfigManager.RemoteServer);
            }

            foreach (Shadowsocks server in ServerManager.ServerList)
            {
                ServerManager.Stop(server);
            }

            // restore
            if (ConfigManager.RemoteServer != null)
            {
                ServerManager.ServerList.Add(ConfigManager.RemoteServer);
            }

            ConfigManager.IsServerPoolEnabled = false;
            InterfaceCtrl.UpdateTransmitLock();
        }
    }
}
