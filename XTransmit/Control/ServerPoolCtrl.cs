using XTransmit.Model;
using XTransmit.Model.SS;
using XTransmit.Utility;

namespace XTransmit.Control
{
    // NOTE - Shadowsocks only
    internal static class ServerPoolCtrl
    {
        public static void StartServerPool()
        {
            if (ServerManager.ShadowsocksList.Count < 1)
            {
                return;
            }

            foreach (Shadowsocks server in ServerManager.ShadowsocksList)
            {
                int listen = NetworkUtil.GetAvailablePort(2000);
                if (listen > 0)
                {
                    ServerManager.AddProcess(server, listen);
                }
            }

            SettingManager.IsServerPoolEnabled = true;
            InterfaceCtrl.UpdateTransmitLock();
        }

        public static void StopServerPool()
        {
            // exclude the transmit server
            if (SettingManager.RemoteServer is Shadowsocks)
            {
                ServerManager.ShadowsocksList.Remove((Shadowsocks)SettingManager.RemoteServer);
            }

            foreach (Shadowsocks server in ServerManager.ShadowsocksList)
            {
                ServerManager.RemoveProcess(server);
            }

            // restore
            if (SettingManager.RemoteServer is Shadowsocks)
            {
                ServerManager.ShadowsocksList.Add((Shadowsocks)SettingManager.RemoteServer);
            }

            SettingManager.IsServerPoolEnabled = false;
            InterfaceCtrl.UpdateTransmitLock();
        }
    }
}
