using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XTransmit.Control;
using XTransmit.Model.SS;
using XTransmit.Model.V2Ray;
using XTransmit.Utility;

namespace XTransmit.Model
{
    /**
     * TODO - Optimize server pool
     */
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class ServerManager
    {
        public static readonly Dictionary<string, Process> ServerProcessMap = new Dictionary<string, Process>();

        public static List<Shadowsocks> ShadowsocksList;
        public static List<V2RayVMess> V2RayList;

        private static readonly Random RandGen = new Random();
        private static readonly object locker = new object();

        private static string pathShadowsocksXml;
        private static string pathV2RayXml;

        // Init server list by deserialize xml file
        public static void Load(string pathShadowsocksXml, string pathV2RayXml)
        {
            // load shadowsocks
            if (FileUtil.XmlDeserialize(pathShadowsocksXml, typeof(List<Shadowsocks>)) is List<Shadowsocks> listShadowsocks)
            {
                ShadowsocksList = listShadowsocks;
            }
            else
            {
                ShadowsocksList = new List<Shadowsocks>();
            }

            // load v2ray
            if (FileUtil.XmlDeserialize(pathV2RayXml, typeof(List<V2RayVMess>)) is List<V2RayVMess> listV2Ray)
            {
                V2RayList = listV2Ray;
            }
            else
            {
                V2RayList = new List<V2RayVMess>();
            }

            ServerManager.pathShadowsocksXml = pathShadowsocksXml;
            ServerManager.pathV2RayXml = pathV2RayXml;
        }

        public static void Save(List<Shadowsocks> listShadowsocks)
        {
            FileUtil.XmlSerialize(pathShadowsocksXml, listShadowsocks);
            ShadowsocksList = listShadowsocks;
        }

        public static void Save(List<V2RayVMess> listV2Ray)
        {
            List<V2RayVMess> asdfsdaf = listV2Ray.Cast<V2RayVMess>().ToList();
            FileUtil.XmlSerialize(pathV2RayXml, asdfsdaf);
            V2RayList = listV2Ray;
        }

        // TODO - Server type (SS, V2Ray ...)
        public static bool Start(BaseServer server, int listen)
        {
            if (ServerProcessMap.ContainsKey(server.GetId()))
            {
                return true;
            }

            if (server is Shadowsocks shadowsocks)
            {
                if (ProcSS.Execute(shadowsocks, listen) is Process process)
                {
                    shadowsocks.ListenPort = listen;
                    ServerProcessMap.Add(shadowsocks.GetId(), process);
                    return true;
                }
            }

            return false;
        }

        public static void Stop(BaseServer server)
        {
            // server is null at the first time running
            if (server == null)
            {
                return;
            }

            if (ServerProcessMap.ContainsKey(server.GetId()))
            {
                Process process = ServerProcessMap[server.GetId()];
                ProcSS.Exit(process);

                server.ListenPort = -1;
                ServerProcessMap.Remove(server.GetId());
            }
        }

        // Server Pool 
        public static Shadowsocks GerRendom()
        {
            lock (locker)
            {
                if (ServerProcessMap.Count > 1)
                {
                    int index = RandGen.Next(0, ServerProcessMap.Count - 1);
                    string id = ServerProcessMap.Keys.ElementAt(index);
                    return ShadowsocksList.FirstOrDefault(server => server.GetId() == id);
                }
                else if (ServerProcessMap.Count > 0)
                {
                    string id = ServerProcessMap.Keys.ElementAt(0);
                    return ShadowsocksList.FirstOrDefault(server => server.GetId() == id);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
