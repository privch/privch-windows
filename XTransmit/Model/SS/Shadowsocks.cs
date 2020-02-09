using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using XTransmit.Utility;

namespace XTransmit.Model.SS
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public class Shadowsocks : BaseServer
    {
        #region Public-Static
        // encrypt method
        public static List<string> Ciphers { get; } = new List<string>
        {
            "rc4-md5",
            "aes-128-gcm", "aes-192-gcm", "aes-256-gcm",
            "aes-128-cfb", "aes-192-cfb", "aes-256-cfb",
            "aes-128-ctr", "aes-192-ctr", "aes-256-ctr",
            "bf-cfb",
            "camellia-128-cfb", "camellia-192-cfb", "camellia-256-cfb",
            "chacha20", "chacha20-ietf", "chacha20-ietf-poly1305",
            "xchacha20-ietf-poly1305",
            "salsa20",
        };
        #endregion

        // deserializer need to set property
        #region Properties(Serializable)
        public string Encrypt
        {
            get => encrypt;
            set
            {
                encrypt = value;
                OnPropertyChanged(nameof(Encrypt));
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string Remarks
        {
            get => remarks;
            set
            {
                remarks = value;
                OnPropertyChanged(nameof(Remarks));
            }
        }

        public bool PluginEnabled
        {
            get => pluginEnabled;
            set
            {
                pluginEnabled = value;
                OnPropertyChanged(nameof(PluginEnabled));
            }
        }

        public string PluginName
        {
            get => pluginName;
            set
            {
                pluginName = value;
                OnPropertyChanged(nameof(PluginName));
            }
        }
        public string PluginOption
        {
            get => pluginOption;
            set
            {
                pluginOption = value;
                OnPropertyChanged(nameof(PluginOption));
            }
        }
        #endregion 

        // values 
        private string encrypt;
        private string password;
        private string remarks;

        private bool pluginEnabled;
        private string pluginName;
        private string pluginOption;

        // call after the ConfigManager.Global has been loaded
        public Shadowsocks()
        {
            encrypt = "chacha20-ietf-poly1305";
            password = string.Empty;
            remarks = string.Empty;

            pluginEnabled = false;
            pluginName = string.Empty;
            pluginOption = string.Empty;
        }

        public Shadowsocks Copy()
        {
            return (Shadowsocks)TextUtil.CopyBySerializer(this);
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = string.IsNullOrWhiteSpace(Remarks) ? $"{HostAddress} - {HostPort}" : Remarks;
        }


        #region Fectory
        private static readonly Regex
            UrlFinder = new Regex(@"ss://(?<base64>[A-Za-z0-9+-/=_]+)(?:#(?<tag>\S+))?", RegexOptions.IgnoreCase),
            DetailsParser = new Regex(@"^((?<method>.+?):(?<password>.*)@(?<hostname>.+?):(?<port>\d+?))$", RegexOptions.IgnoreCase);

        /**
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
        #endregion
    }
}
