using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;

namespace XTransmit.Model.Server
{
    /**TODO Next - Move ServerProfile data to ServerManager
     * 
     * Reference code: 
     * https://github.com/shadowsocks/shadowsocks-windows/raw/master/shadowsocks-csharp/Model/Server.cs
     * 
     * Updated: 2019-08-04
     */
    public static class ServerManager
    {
        // Init server list by deserialize xml file
        public static List<ServerProfile> LoadServer(string fileServerXml)
        {
            List<ServerProfile> serverList;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ServerProfile>));
            try
            {
                FileStream fileStream = new FileStream(fileServerXml, FileMode.Open);
                serverList = (List<ServerProfile>)xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception)
            {
                serverList = null;
            }

            return serverList;
        }

        public static void SaveServer(string fileServerXml, List<ServerProfile> listServerProfile)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ServerProfile>));
            try
            {
                StreamWriter writer = new StreamWriter(fileServerXml);
                xmlSerializer.Serialize(writer, listServerProfile);
                writer.Close();
            }
            catch (Exception) { }
        }

        // start with "ss://"
        public static ServerProfile ParseLegacyServer(string ssUrl)
        {
            var match = UrlFinder.Match(ssUrl);
            if (!match.Success)
                return null;

            ServerProfile serverProfile = ServerProfile.Default();
            var base64 = match.Groups["base64"].Value.TrimEnd('/');
            var tag = match.Groups["tag"].Value;
            if (!string.IsNullOrEmpty(tag))
            {
                serverProfile.vRemarks = HttpUtility.UrlDecode(tag, Encoding.UTF8);
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
                return null;

            serverProfile.vEncrypt = details.Groups["method"].Value;
            serverProfile.vPassword = details.Groups["password"].Value;
            serverProfile.vHostIP = details.Groups["hostname"].Value;
            serverProfile.vPort = int.Parse(details.Groups["port"].Value);

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
                    continue;

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

                    serverProfile = ServerProfile.Default();
                    serverProfile.vHostIP = parsedUrl.IdnHost;
                    serverProfile.vPort = parsedUrl.Port;
                    serverProfile.vRemarks = parsedUrl.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);

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

                    serverProfile.vEncrypt = userInfoParts[0];
                    serverProfile.vPassword = userInfoParts[1];

                    // plugin
                    NameValueCollection queryParameters = HttpUtility.ParseQueryString(parsedUrl.Query);
                    string[] pluginParts = HttpUtility.UrlDecode(queryParameters["plugin"] ?? "").Split(new[] { ';' }, 2);
                    if (pluginParts.Length > 0)
                    {
                        serverProfile.vPluginName = pluginParts[0] ?? "";
                    }
                    if (pluginParts.Length > 1)
                    {
                        serverProfile.vPluginOption = pluginParts[1] ?? "";
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