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

        private bool pluginEnabled;
        private string pluginName;
        private string pluginOption;

        // call after the SettingManager.Configuration has been loaded
        public Shadowsocks()
        {
            encrypt = "chacha20-ietf-poly1305";
            password = string.Empty;

            pluginEnabled = false;
            pluginName = string.Empty;
            pluginOption = string.Empty;
        }

        public Shadowsocks Copy()
        {
            return (Shadowsocks)TextUtil.CopyBySerializer(this);
        }

        #region Import
        private static readonly Regex
            UrlFinder = new Regex(@"ss://(?<base64>[A-Za-z0-9+-/=_]+)(?:#(?<tag>\S+))?", RegexOptions.IgnoreCase),
            DetailsParser = new Regex(@"^((?<method>.+?):(?<password>.*)@(?<hostname>.+?):(?<port>\d+?))$", RegexOptions.IgnoreCase);

        /**<summary>
         * Split ssBase64List by "\r\n", "\r", "\n", " "
         * </summary>
         */
        public static List<Shadowsocks> ImportServers(string ssBase64List)
        {
            List<Shadowsocks> serverList = new List<Shadowsocks>();
            if (string.IsNullOrWhiteSpace(ssBase64List))
            {
                return serverList;
            }

            // split items
            string[] ssBaseItems = ssBase64List.Split(
                new string[] { "\r\n", "\r", "\n", " " },
                StringSplitOptions.RemoveEmptyEntries);

            // parse items
            foreach (string ssBase64Item in ssBaseItems)
            {
                string ssBase64 = ssBase64Item.Trim();
                if (!ssBase64.StartsWith("ss://", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Shadowsocks server = ParseLegacyServer(ssBase64);
                if (server != null)   //legacy
                {
                    serverList.Add(server);
                }
                else   //SIP002
                {
                    Uri parsedUrl;
                    try
                    {
                        parsedUrl = new Uri(ssBase64);
                    }
                    catch (UriFormatException)
                    {
                        continue;
                    }

                    server = new Shadowsocks
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

                    server.Encrypt = userInfoParts[0];
                    server.Password = userInfoParts[1];

                    // plugin
                    NameValueCollection queryParameters = HttpUtility.ParseQueryString(parsedUrl.Query);
                    string[] pluginParts = HttpUtility.UrlDecode(queryParameters["plugin"] ?? "").Split(new[] { ';' }, 2);
                    if (pluginParts.Length > 0)
                    {
                        server.PluginName = pluginParts[0] ?? "";
                    }
                    if (pluginParts.Length > 1)
                    {
                        server.PluginOption = pluginParts[1] ?? "";
                    }

                    server.SetFriendlyNameDefault();
                    serverList.Add(server);
                }
            }

            return serverList;
        }

        /**
         * start with "ss://". 
         * Reference code: github.com/shadowsocks/shadowsocks-windows/raw/master/shadowsocks-csharp/Model/Server.cs
         */
        public static Shadowsocks ParseLegacyServer(string ssBase64)
        {
            var match = UrlFinder.Match(ssBase64);
            if (!match.Success)
            {
                return null;
            }

            Shadowsocks server = new Shadowsocks();
            var base64 = match.Groups["base64"].Value.TrimEnd('/');
            var tag = match.Groups["tag"].Value;
            if (!string.IsNullOrEmpty(tag))
            {
                server.Remarks = HttpUtility.UrlDecode(tag, Encoding.UTF8);
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
                server.HostPort = int.Parse(details.Groups["port"].Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }

            server.Encrypt = details.Groups["method"].Value;
            server.Password = details.Groups["password"].Value;
            server.HostAddress = details.Groups["hostname"].Value;

            server.SetFriendlyNameDefault();
            return server;
        }
        #endregion
    }
}
