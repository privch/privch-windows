using System.Collections.Generic;

using PrivCh.Model;
using PrivCh.Model.SS;
using PrivCh.Utility;

namespace PrivCh.Control
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

            List<int> portInUse = NetworkUtil.GetPortInUse(2000);
            foreach (Shadowsocks server in ServerManager.ShadowsocksList)
            {
                int listen = NetworkUtil.GetAvailablePort(2000, portInUse);
                if (listen > 0)
                {
                    ServerManager.AddProcess(server, listen);
                    portInUse.Add(listen);
                }
            }

            SettingManager.IsServerPoolEnabled = true;
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
        }
    }
}
