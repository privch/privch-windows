using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace XTransmit.Utility
{
    /**
     * Updated: 2019-08-02
     */
    public static class NetworkUtil
    {
        public static int CompareNetworkInterfaceBySpeed(NetworkInterface x, NetworkInterface y) => (int)(x.Speed - y.Speed);

        public static List<NetworkInterface> GetValidNetworkInterface()
        {
            List<NetworkInterface> adapterList = new List<NetworkInterface>();

            NetworkInterface[] networkInterfaces;
            try
            {
                networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            }
            catch (Exception)
            {
                networkInterfaces = null;
            }

            if (networkInterfaces == null || networkInterfaces.Length < 1)
                return adapterList;

            foreach (NetworkInterface adapter in networkInterfaces)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;

                adapterList.Add(adapter);
            }

            return adapterList;
        }

        /** 
         * <summary>
         * Checks for used ports and retrieves a random free port
         * </summary>
         * <returns>The free port or 0 if it did not find a free port</returns>
         */
        public static int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            // getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            Random random = new Random();
            for (int i = startingPort; i < UInt16.MaxValue; i++) // random enough times
            {
                int port = random.Next(startingPort, UInt16.MaxValue);
                if (!portArray.Contains(port))
                    return port;
            }

            return 0;
        }
    }
}
