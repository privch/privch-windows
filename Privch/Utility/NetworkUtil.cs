using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;

namespace Privch.Utility
{
    public static class NetworkUtil
    {
        public static int CompareNetworkInterfaceBySpeed(NetworkInterface x, NetworkInterface y)
        {
            return (int)(x?.Speed ?? 0 - y?.Speed ?? 0);
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static List<NetworkInterface> GetValidNetworkInterface()
        {
            List<NetworkInterface> adapterList = new List<NetworkInterface>();

            NetworkInterface[] networkInterfaces;
            try
            {
                networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            }
            catch
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

        // set dns for current adapter
        public static void SetDNS(string[] dns)
        {
            using (ManagementClass manClass = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                ManagementObjectCollection manObjC = manClass.GetInstances();
                foreach (ManagementObject manObj in manObjC)
                {
                    if ((bool)manObj["IPEnabled"])
                    {
                        ManagementBaseObject inParams = manObj.GetMethodParameters("SetDNSServerSearchOrder");
                        inParams["DNSServerSearchOrder"] = dns;
                        ManagementBaseObject outParams = manObj.InvokeMethod("SetDNSServerSearchOrder", inParams, null);
                    }
                }
            }
        }

        public static List<int> GetPortInUse(int startPort)
        {
            IPEndPoint[] endPoints;
            List<int> portList = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            // getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portList.AddRange(from n in connections
                              where n.LocalEndPoint.Port >= startPort
                              select n.LocalEndPoint.Port);

            // getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portList.AddRange(from n in endPoints
                              where n.Port >= startPort
                              select n.Port);

            // getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portList.AddRange(from n in endPoints
                              where n.Port >= startPort
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
        public static int GetAvailablePort(int startPort, List<int> exceptPort = null)
        {
            if (exceptPort == null)
            {
                exceptPort = GetPortInUse(startPort);
            }

            Random random = new Random();
            for (int i = startPort; i < ushort.MaxValue; i++) // random enough times
            {
                int port = random.Next(startPort, ushort.MaxValue);
                if (!exceptPort.Contains(port))
                {
                    return port;
                }
            }

            return 0;
        }
    }
}
