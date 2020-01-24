using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using XTransmit.Utility;

namespace XTransmit.Model.Server
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class ServerManager
    {
        public static readonly Dictionary<ServerProfile, Process> ServerProcessMap = new Dictionary<ServerProfile, Process>();
        public static List<ServerProfile> ServerList { get; private set; }

        private static string ServerXmlPath;
        private static readonly Random RandGen = new Random();
        private static readonly object locker = new object();

        // Init server list by deserialize xml file
        public static void Load(string pathServerXml)
        {
            if (FileUtil.XmlDeserialize(pathServerXml, typeof(List<ServerProfile>)) is List<ServerProfile> listServer)
            {
                ServerList = listServer;
            }
            else
            {
                ServerList = new List<ServerProfile>();
            }

            ServerXmlPath = pathServerXml;
        }

        public static void Save(List<ServerProfile> listServerProfile)
        {
            FileUtil.XmlSerialize(ServerXmlPath, listServerProfile);
            ServerList = listServerProfile;
        }

        // TODO - Server type (SS, V2Ray ...)
        public static bool Start(ServerProfile server, int listen)
        {
            if (ServerProcessMap.ContainsKey(server))
            {
                return true;
            }

            if (SSManager.Execute(server, listen) is Process process)
            {
                ServerProcessMap.Add(server, process);
                server.ListenPort = listen;
                return true;
            }

            return false;
        }

        public static void Stop(ServerProfile server)
        {
            // server is null at the first time running
            if (server != null && ServerProcessMap.ContainsKey(server))
            {
                Process process = ServerProcessMap[server];
                SSManager.Exit(process);

                server.ListenPort = -1;
                ServerProcessMap.Remove(server);
            }
        }

        // Server Pool 
        public static ServerProfile GerRendom()
        {
            lock (locker)
            {
                if (ServerProcessMap.Count > 1)
                {
                    int index = RandGen.Next(0, ServerProcessMap.Count - 1);
                    return ServerProcessMap.Keys.ElementAt(index);
                }
                else if (ServerProcessMap.Count > 0)
                {
                    return ServerProcessMap.Keys.ElementAt(0);
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
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "<Pending>")]
        public static ServerProfile ParseLegacyServer(string ssUrl)
        {
            var match = UrlFinder.Match(ssUrl);
            if (!match.Success)
            {
                return null;
            }

            ServerProfile serverProfile = new ServerProfile();
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
            catch (Exception)
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
            serverProfile.HostIP = details.Groups["hostname"].Value;

            serverProfile.SetFriendlyNameDefault();
            return serverProfile;
        }

        /**<summary>
         * The "serverInfo" will be splited by "\r\n", "\r", "\n", " "
         * </summary>
         * <returns>Return number of server added</returns>
         */
        public static List<ServerProfile> ImportServers(string serverInfos)
        {
            string[] serverInfoArray = serverInfos.Split(new string[] { "\r\n", "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);

            List<ServerProfile> serverList = new List<ServerProfile>();
            foreach (string serverInfo in serverInfoArray)
            {
                string serverUrl = serverInfo.Trim();
                if (!serverUrl.StartsWith("ss://", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                ServerProfile serverProfile = ParseLegacyServer(serverUrl);
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

                    serverProfile = new ServerProfile
                    {
                        HostIP = parsedUrl.IdnHost,
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
                    catch (Exception)
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
