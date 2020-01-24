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

            App.GlobalConfig.IsServerPoolEnabled = true;
        }

        public static void StopServerPool()
        {
            // exclude the transmit server
            if (App.GlobalConfig.IsTransmitEnabled)
            {
                ServerManager.ServerList.Remove(App.GlobalConfig.RemoteServer);
            }

            foreach (ServerProfile server in ServerManager.ServerList)
            {
                ServerManager.Stop(server);
            }

            // restore
            if (App.GlobalConfig.IsTransmitEnabled)
            {
                ServerManager.ServerList.Add(App.GlobalConfig.RemoteServer);
            }

            App.GlobalConfig.IsServerPoolEnabled = false;
        }
    }
}
