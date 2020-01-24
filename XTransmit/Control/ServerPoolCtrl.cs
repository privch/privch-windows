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

            foreach (ServerProfile server in ServerManager.ServerList)
            {
                int listen = NetworkUtil.GetAvailablePort(2000);
                if (listen > 0)
                {
                    ServerManager.Start(server, listen);
                }
            }

            ConfigManager.IsServerPoolEnabled = true;
        }

        public static void StopServerPool()
        {
            // exclude the transmit server
            if (ConfigManager.Global.IsTransmitEnabled)
            {
                ServerManager.ServerList.Remove(ConfigManager.Global.RemoteServer);
            }

            foreach (ServerProfile server in ServerManager.ServerList)
            {
                ServerManager.Stop(server);
            }

            // restore
            if (ConfigManager.Global.IsTransmitEnabled)
            {
                ServerManager.ServerList.Add(ConfigManager.Global.RemoteServer);
            }

            ConfigManager.IsServerPoolEnabled = false;
        }
    }
}
