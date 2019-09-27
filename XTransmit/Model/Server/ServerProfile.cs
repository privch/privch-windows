using System;
using System.Net.NetworkInformation;
using System.Text;
using XTransmit.Model.IPAddress;

namespace XTransmit.Model.Server
{
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
        public string vHostIP;
        public int vPort;
        public string vEncrypt;
        public string vPassword;
        public string vRemarks;

        public bool vPluginEnabled;
        public string vPluginName;
        public string vPluginOption;

        // preference
        public string vFriendlyName;
        public int vTimeout;

        // info
        public string vTimeCreated;
        public long vPing; // less then 0 means timeout or unreachable
        public IPInfo vIPData;

        public ServerProfile Copy() => new ServerProfile
        {
            vHostIP = vHostIP,
            vPort = vPort,
            vEncrypt = vEncrypt,
            vPassword = vPassword,
            vRemarks = vRemarks,

            vPluginEnabled = vPluginEnabled,
            vPluginName = vPluginName,
            vPluginOption = vPluginOption,

            vFriendlyName = vFriendlyName,
            vTimeout = vTimeout,

            vTimeCreated = vTimeCreated,
            vPing = vPing,
            vIPData = vIPData?.Copy(),
        };

        /**<summary>
         * Must be called after the App.GlobalConfig has been loaded
         * </summary> 
         */
        public ServerProfile()
        {
            vHostIP = "";
            vPort = 0;
            vEncrypt = "chacha20-ietf-poly1305";
            vPassword = "";
            vRemarks = "";

            vPluginEnabled = false;
            vPluginName = "";
            vPluginOption = "";

            vFriendlyName = "";
            vTimeout = App.GlobalConfig != null ? App.GlobalConfig.ConnectionTimeouts : 3;

            vTimeCreated = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            vPing = 0;
            vIPData = null;
        }

        public void SetFriendlyNameDefault()
        {
            vFriendlyName = string.IsNullOrWhiteSpace(vRemarks) ? $"{vHostIP} - {vPort}" : vRemarks;
        }

        public void SetFriendNameByIPData()
        {
            if (vIPData == null)
                return;

            StringBuilder stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(vIPData.country))
                stringBuilder.Append(vIPData.country);

            if (!string.IsNullOrWhiteSpace(vIPData.region))
                stringBuilder.Append(" - " + vIPData.region);

            if (!string.IsNullOrWhiteSpace(vIPData.city))
                stringBuilder.Append(" - " + vIPData.city);

            string friendlyName = stringBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(friendlyName))
            {
                if (friendlyName.StartsWith(" - "))
                    friendlyName = friendlyName.Substring(3);

                vFriendlyName = friendlyName;
            }
        }

        public void FetchIPData(bool focus)
        {
            if (vIPData == null || focus)
            {
                vIPData = IPInfo.Fetch(vHostIP);
            }
        }

        // OO Programming
        public void FetchPingData(Ping ping)
        {
            try
            {
                PingReply reply = ping.Send(vHostIP, 5000);
                vPing = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
            }
            catch (Exception)
            {
                vPing = -1;
            }
        }

        /** Serializable ==================================================
         */
        public override int GetHashCode() => vHostIP.GetHashCode() ^ vPort;

        public override bool Equals(object serverNew)
        {
            if (serverNew is ServerProfile server)
            {
                return vHostIP == server.vHostIP && vPort == server.vPort;
            }
            else
            {
                return false;
            }
        }
    }
}
