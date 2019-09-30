using System;
using System.Net.NetworkInformation;
using System.Text;
using XTransmit.Model.IPAddress;
using XTransmit.Utility;

namespace XTransmit.Model.Server
{
    /**
     * Updated: 2019-09-29
     */
    [Serializable]
    public class ServerProfile
    {
        // constants
        public static readonly string[] Ciphers =
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

        // values
        public string HostIP;
        public int HostPort;
        public string Encrypt;
        public string Password;
        public string Remarks;

        public bool PluginEnabled;
        public string PluginName;
        public string PluginOption;

        // preference and data
        public string FriendlyName;
        public int ListenPort;
        public int Timeout;
        public long Ping; // less then 0 means timeout or unreachable

        // info
        public string TimeCreated;
        public IPInfo IPData;

        public ServerProfile Copy()
        {
            return (ServerProfile)TextUtil.CopyBySerializer(this);
        }

        /**<summary>
         * Must be called after the App.GlobalConfig has been loaded
         * </summary> 
         */
        public ServerProfile()
        {
            HostIP = "";
            HostPort = 0;
            Encrypt = "chacha20-ietf-poly1305";
            Password = "";
            Remarks = "";

            PluginEnabled = false;
            PluginName = "";
            PluginOption = "";

            FriendlyName = "";
            ListenPort = -1;
            Timeout = App.GlobalConfig != null ? App.GlobalConfig.ConnectionTimeouts : 3;
            Ping = 0;

            TimeCreated = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            IPData = null;
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = string.IsNullOrWhiteSpace(Remarks) ? $"{HostIP} - {HostPort}" : Remarks;
        }

        public void SetFriendNameByIPData()
        {
            if (IPData == null)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(IPData.country))
                stringBuilder.Append(IPData.country);

            if (!string.IsNullOrWhiteSpace(IPData.region))
                stringBuilder.Append(" - " + IPData.region);

            if (!string.IsNullOrWhiteSpace(IPData.city))
                stringBuilder.Append(" - " + IPData.city);

            string friendlyName = stringBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(friendlyName))
            {
                if (friendlyName.StartsWith(" - "))
                    friendlyName = friendlyName.Substring(3);

                FriendlyName = friendlyName;
            }
        }

        public void FetchIPData(bool focus)
        {
            if (IPData == null || focus)
            {
                IPData = IPInfo.Fetch(HostIP);
            }
        }

        // OO Programming
        public void FetchPingData(Ping ping)
        {
            try
            {
                PingReply reply = ping.Send(HostIP, 5000);
                Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
            }
            catch (Exception)
            {
                Ping = -1;
            }
        }

        /** Serializable ==================================================
         */
        public override int GetHashCode() => HostIP.GetHashCode() ^ HostPort;

        public override bool Equals(object serverNew)
        {
            if (serverNew is ServerProfile server)
            {
                return HostIP == server.HostIP && HostPort == server.HostPort;
            }
            else
            {
                return false;
            }
        }
    }
}
