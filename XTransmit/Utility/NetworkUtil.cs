using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace XTransmit.Utility
{
    /**
     * Updated: 2019-09-30
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
            {
                return adapterList;
            }

            foreach (NetworkInterface adapter in networkInterfaces)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                if (adapter.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                adapterList.Add(adapter);
            }

            return adapterList;
        }

        public static List<int> GetPortInUse(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portList = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            // getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portList.AddRange(from n in connections
                              where n.LocalEndPoint.Port >= startingPort
                              select n.LocalEndPoint.Port);

            // getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portList.AddRange(from n in endPoints
                              where n.Port >= startingPort
                              select n.Port);

            // getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portList.AddRange(from n in endPoints
                              where n.Port >= startingPort
                              select n.Port);

            portList.Sort();
            return portList;
        }

        /** 
         * <summary>
         * Checks for used ports and retrieves a random free port
         * </summary>
         * <returns>The free port or 0 if it did not find a free port</returns>
         */
        public static int GetAvailablePort(int startingPort, List<int> exceptPort = null)
        {
            if (exceptPort == null)
            {
                exceptPort = GetPortInUse(startingPort);
            }

            Random random = new Random();
            for (int i = startingPort; i < ushort.MaxValue; i++) // random enough times
            {
                int port = random.Next(startingPort, ushort.MaxValue);
                if (!exceptPort.Contains(port))
                {
                    return port;
                }
            }

            return 0;
        }
    }
}
