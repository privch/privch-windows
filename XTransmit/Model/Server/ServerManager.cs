using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using XTransmit.Utility;

namespace XTransmit.Model.Server
{
    /**
     * Updated: 2019-09-24
     */
    public static class ServerManager
    {
        public static List<ServerProfile> ServerList;
        private static string ServerXmlPath;

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
        }


        /** Import ---------------------------------------------------------------------------------------
         * start with "ss://". 
         * Reference code: github.com/shadowsocks/shadowsocks-windows/raw/master/shadowsocks-csharp/Model/Server.cs
         */
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

            Match details = null;
            try
            {
                details = DetailsParser.Match(
                    Encoding.UTF8.GetString(Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='))));
            }
            catch (FormatException)
            {
                return null;
            }

            if (!details.Success)
            {
                return null;
            }

            serverProfile.Encrypt = details.Groups["method"].Value;
            serverProfile.Password = details.Groups["password"].Value;
            serverProfile.HostIP = details.Groups["hostname"].Value;
            serverProfile.HostPort = int.Parse(details.Groups["port"].Value);

            serverProfile.SetFriendlyNameDefault();
            return serverProfile;
        }

        /**<summary>
         * The "serverInfo" will be splited by "\r\n", "\r", "\n", " "
         * </summary>
         * <returns>Return count added</returns>
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
                    catch (FormatException)
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
