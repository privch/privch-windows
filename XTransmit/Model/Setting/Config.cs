using System;

namespace XTransmit.Model.Setting
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
        public int GlobalSocks5Port { get; set; }
        public string RemoteServerType { get; set; }
        public string RemoteServerID { get; set; }

        // timeouts 
        public int SSTimeout { get; set; }
        public int IPInfoConnTimeout { get; set; }
        public int ResponseConnTimeout { get; set; } //not used
        public int PingTimeout { get; set; } //ms

        // server
        public bool IsReplaceOldServer { get; set; }

        public Config()
        {
            IsAutorun = true;

            IsTransmitEnabled = false;
            SystemProxyPort = 0;
            GlobalSocks5Port = 0;
            RemoteServerType = null;
            RemoteServerID = null;

            SSTimeout = 5;
            IPInfoConnTimeout = 6;
            ResponseConnTimeout = 6;
            PingTimeout = 3000;

            IsReplaceOldServer = false;
        }
    }
}
