using System;

namespace PrivCh.Model.Setting
{
    /**<summary>
     * Feature options. Such as SystemProxy, Privoxy, Shadowsocks.
     * </summary> 
     */
    [Serializable]
    public class Config
    {
        public bool IsAutorun { get; set; }

        // transmit
        public bool IsTransmitEnabled { get; set; }
        public int SystemProxyPort { get; set; }
        public int LocalSocks5Port { get; set; }
        public string RemoteServerType { get; set; }
        public string RemoteServerID { get; set; }

        // timeouts 
        public int TimeoutShadowsocks { get; set; }
        public int TimeoutUpdateInfo { get; set; }
        public int TimeoutCheckResponse { get; set; }
        public int TimeoutPing { get; set; }

        // server
        public bool IsReplaceOldServer { get; set; }

        public Config()
        {
            IsAutorun = false;

            IsTransmitEnabled = false;
            SystemProxyPort = 0;
            LocalSocks5Port = 0;
            RemoteServerType = null;
            RemoteServerID = null;

            TimeoutShadowsocks = 5;
            TimeoutUpdateInfo = 10000; //ms
            TimeoutCheckResponse = 20000; //ms
            TimeoutPing = 1000; //ms

            IsReplaceOldServer = true;
        }
    }
}
