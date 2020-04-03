using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Privch.Control;
using Privch.Model.SS;
using Privch.Model.V2Ray;
using Privch.Utility;

namespace Privch.Model
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

        private static HttpListener httpListener = null;
        private static string pathShadowsocksXml;
        private static string pathV2RayXml;

        // Init server list by deserialize xml file
        public static void Initialize(string pathShadowsocksXml, string pathV2RayXml)
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

            // server pool service
            StartHttpShow();
        }
        public static void Dispose()
        {
            StopHttpShow();
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

        public static bool AddProcess(BaseServer server, int listen)
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
            else if (server is V2RayVMess v2rayVMess)
            {
                if (ProcV2Ray.Start(v2rayVMess, listen))
                {
                    v2rayVMess.ListenPort = listen;
                    ServerProcessMap.Add(v2rayVMess.GetId(), null);
                    return true;
                }
            }

            return false;
        }

        public static void RemoveProcess(BaseServer server)
        {
            // server is null at the first time running
            if (server == null || !ServerProcessMap.ContainsKey(server.GetId()))
            {
                return;
            }

            if (server is Shadowsocks shadowsocks)
            {
                Process process = ServerProcessMap[shadowsocks.GetId()];
                ProcSS.Exit(process);

                shadowsocks.ListenPort = -1;
                ServerProcessMap.Remove(shadowsocks.GetId());
            }
            else if (server is V2RayVMess v2rayVMess)
            {
                ProcV2Ray.Stop();
                v2rayVMess.ListenPort = -1;
                ServerProcessMap.Remove(v2rayVMess.GetId());
            }
        }

        #region ServerPool-Show
        // http service
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static void StartHttpShow()
        {
            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add("http://127.0.0.1:44100/");
                httpListener.Start();
                httpListener.BeginGetContext(HttpShowServe, null);
            }
            catch
            {
                httpListener?.Close();
            }
        }

        private static void StopHttpShow()
        {
            try
            {
                httpListener?.Stop();
            }
            catch { }
            httpListener?.Close();
        }

        // service
        private static void HttpShowServe(IAsyncResult result)
        {
            try
            {
                HttpListenerContext context = httpListener.EndGetContext(result);
                HttpListenerResponse response = context.Response;

                // response content
                string content = null;
                foreach (Shadowsocks shadowsocks in ShadowsocksList)
                {
                    if (shadowsocks.ListenPort > 0)
                    {
                        content += $"socks5://127.0.0.1:{shadowsocks.ListenPort}\r\n";
                    }
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);

                // continue serve
                httpListener.BeginGetContext(HttpShowServe, null);
            }
            catch { }
        }
        #endregion
    }
}
