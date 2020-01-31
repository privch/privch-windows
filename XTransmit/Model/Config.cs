using System;
using XTransmit.Model.Server;
using XTransmit.Utility;

namespace XTransmit.Model
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
        public ServerProfile RemoteServer { get; set; }

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
            RemoteServer = null;

            SSTimeout = 5;
            IPInfoConnTimeout = 6;
            ResponseConnTimeout = 6;
            PingTimeout = 3000;

            IsReplaceOldServer = false;
        }
    }

    internal static class ConfigManager
    {
        public static Config Global;

        //status
        public static bool IsServerPoolEnabled = false;

        /**<summary>
         * Object is constructed by serializer with default values,
         * property (which also specified in the XML) value will be overwritten from the XML
         * </summary>
         */
        public static void LoadFileOrDefault(string pathConfigXml)
        {
            if (FileUtil.XmlDeserialize(pathConfigXml, typeof(Config)) is Config config)
            {
                Global = config;
            }
            else
            {
                Global = new Config();
            }
        }

        public static void WriteFile(string pathConfigXml)
        {
            FileUtil.XmlSerialize(pathConfigXml, Global);
        }
    }
}
