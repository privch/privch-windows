using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using XTransmit.Model.V2Ray;
using XTransmit.Utility;

namespace XTransmit.Model.Server
{
    /**
     * TODO - Optimize server pool
     */
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class ServerManager
    {
        public static readonly Dictionary<string, Process> ServerProcessMap = new Dictionary<string, Process>();

        public static List<Shadowsocks> ShadowsocksList;
        public static List<V2RayServer> V2RayList;

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
            if (FileUtil.XmlDeserialize(pathV2RayXml, typeof(List<V2RayServer>)) is List<V2RayServer> listV2Ray)
            {
                V2RayList = listV2Ray;
            }
            else
            {
                V2RayList = new List<V2RayServer>();
            }

            ServerManager.pathShadowsocksXml = pathShadowsocksXml;
            ServerManager.pathV2RayXml = pathV2RayXml;
        }

        public static void Save(List<Shadowsocks> listShadowsocks)
        {
            FileUtil.XmlSerialize(pathShadowsocksXml, listShadowsocks);
            ShadowsocksList = listShadowsocks;
        }

        public static void Save(List<V2RayServer> listV2Ray)
        {
            List<V2RayServer> asdfsdaf = listV2Ray.Cast<V2RayServer>().ToList();
            FileUtil.XmlSerialize(pathV2RayXml, asdfsdaf);
            V2RayList = listV2Ray;
        }

        // TODO - Server type (SS, V2Ray ...)
        public static bool Start(IServer server, int listen)
        {
            if (ServerProcessMap.ContainsKey(server.GetID()))
            {
                return true;
            }

            if (server is Shadowsocks shadowsocks)
            {
                if (SSManager.Execute(shadowsocks, listen) is Process process)
                {
                    shadowsocks.ListenPort = listen;
                    ServerProcessMap.Add(shadowsocks.GetID(), process);
                    return true;
                }
            }

            return false;
        }

        public static void Stop(IServer server)
        {
            // server is null at the first time running
            if (server == null)
            {
                return;
            }

            if (ServerProcessMap.ContainsKey(server.GetID()))
            {
                Process process = ServerProcessMap[server.GetID()];
                SSManager.Exit(process);

                server.ListenPort = -1;
                ServerProcessMap.Remove(server.GetID());
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
                    return ShadowsocksList.FirstOrDefault(server => server.GetID() == id);
                }
                else if (ServerProcessMap.Count > 0)
                {
                    string id = ServerProcessMap.Keys.ElementAt(0);
                    return ShadowsocksList.FirstOrDefault(server => server.GetID() == id);
                }
                else
                {
                    return null;
                }
            }
        }

        /** Import ---------------------------------------------------------------------------------------
         * start with "ss://". 
         * Reference code: github.com/shadowsocks/shadowsocks-windows/raw/master/shadowsocks-csharp/Model/Server.cs
         */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "<Pending>")]
        public static Shadowsocks ParseLegacyServer(string ssUrl)
        {
            var match = UrlFinder.Match(ssUrl);
            if (!match.Success)
            {
                return null;
            }

            Shadowsocks serverProfile = new Shadowsocks();
            var base64 = match.Groups["base64"].Value.TrimEnd('/');
            var tag = match.Groups["tag"].Value;
            if (!string.IsNullOrEmpty(tag))
            {
                serverProfile.Remarks = HttpUtility.UrlDecode(tag, Encoding.UTF8);
            }

            Match details;
            try
            {
                details = DetailsParser.Match(
                    Encoding.UTF8.GetString(
                        Convert.FromBase64String(base64.PadRight(base64.Length + (4 - (base64.Length % 4)) % 4, '='))));
            }
            catch
            {
                return null;
            }

            if (!details.Success)
            {
                return null;
            }

            try
            {
                serverProfile.HostPort = int.Parse(details.Groups["port"].Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }

            serverProfile.Encrypt = details.Groups["method"].Value;
            serverProfile.Password = details.Groups["password"].Value;
            serverProfile.HostAddress = details.Groups["hostname"].Value;

            serverProfile.SetFriendlyNameDefault();
            return serverProfile;
        }

        /**<summary>
         * The "serverInfo" will be splited by "\r\n", "\r", "\n", " "
         * </summary>
         * <returns>Return number of server added</returns>
         */
        public static List<Shadowsocks> ImportServers(string serverInfos)
        {
            string[] serverInfoArray = serverInfos.Split(new string[] { "\r\n", "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);

            List<Shadowsocks> serverList = new List<Shadowsocks>();
            foreach (string serverInfo in serverInfoArray)
            {
                string serverUrl = serverInfo.Trim();
                if (!serverUrl.StartsWith("ss://", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                Shadowsocks serverProfile = ParseLegacyServer(serverUrl);
                if (serverProfile != null)   //legacy
                {
                    serverList.Add(serverProfile);
                }
                else   //SIP002
                {
                    Uri parsedUrl;
                    try
                    {
                        parsedUrl = new Uri(serverUrl);
                    }
                    catch (UriFormatException)
                    {
                        continue;
                    }

                    serverProfile = new Shadowsocks
                    {
                        HostAddress = parsedUrl.IdnHost,
                        HostPort = parsedUrl.Port,
                        Remarks = parsedUrl.GetComponents(UriComponents.Fragment, UriFormat.Unescaped)
                    };

                    // parse base64 UserInfo
                    string rawUserInfo = parsedUrl.GetComponents(UriComponents.UserInfo, UriFormat.Unescaped);
                    string base64 = rawUserInfo.Replace('-', '+').Replace('_', '/');    // Web-safe base64 to normal base64
                    string userInfo = "";
                    try
                    {
                        userInfo = Encoding.UTF8.GetString(
                            Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=')));
                    }
                    catch
                    {
                        continue;
                    }

                    string[] userInfoParts = userInfo.Split(new char[] { ':' }, 2);
                    if (userInfoParts.Length != 2)
                    {
                        continue;
                    }

                    serverProfile.Encrypt = userInfoParts[0];
                    serverProfile.Password = userInfoParts[1];

                    // plugin
                    NameValueCollection queryParameters = HttpUtility.ParseQueryString(parsedUrl.Query);
                    string[] pluginParts = HttpUtility.UrlDecode(queryParameters["plugin"] ?? "").Split(new[] { ';' }, 2);
                    if (pluginParts.Length > 0)
                    {
                        serverProfile.PluginName = pluginParts[0] ?? "";
                    }
                    if (pluginParts.Length > 1)
                    {
                        serverProfile.PluginOption = pluginParts[1] ?? "";
                    }

                    serverProfile.SetFriendlyNameDefault();
                    serverList.Add(serverProfile);
                }
            }

            return serverList;
        }

        #region ParseLegacyServer
        public static readonly Regex
            UrlFinder = new Regex(@"ss://(?<base64>[A-Za-z0-9+-/=_]+)(?:#(?<tag>\S+))?", RegexOptions.IgnoreCase),
            DetailsParser = new Regex(@"^((?<method>.+?):(?<password>.*)@(?<hostname>.+?):(?<port>\d+?))$", RegexOptions.IgnoreCase);
        #endregion ParseLegacyServer
    }
}
