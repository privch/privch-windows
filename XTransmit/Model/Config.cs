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
        // transmit
        public bool IsTransmitEnabled { get; set; }
        public bool IsServerPoolEnabled { get; set; }
        public int SystemProxyPort { get; set; }
        public int GlobalSocks5Port { get; set; }
        public ServerProfile RemoteServer { get; set; }

        // timeouts 
        public int SSTimeout { get; set; }
        public int IPInfoConnTimeout { get; set; }
        public int ResponseConnTimeout { get; set; } //not used
        public int PingTimeout { get; set; } //ms

        public bool IsReplaceOldServer { get; set; }

        public string NetworkAdapter { get; set; }

        public Config()
        {
            IsTransmitEnabled = false;
            IsServerPoolEnabled = false;

            SystemProxyPort = 0;
            GlobalSocks5Port = 0;
            RemoteServer = null;

            SSTimeout = 5;
            IPInfoConnTimeout = 6;
            ResponseConnTimeout = 6;
            PingTimeout = 3000;

            IsReplaceOldServer = false;
            NetworkAdapter = null;
        }


        /**<summary>
         * Object is constructed by serializer with default values,
         * property (which also specified in the XML) value will be overwritten from the XML
         * </summary>
         */
        public static Config LoadFileOrDefault(string pathConfigXml)
        {
            if (FileUtil.XmlDeserialize(pathConfigXml, typeof(Config)) is Config config)
            {
                return config;
            }
            else
            {
                return new Config();
            }
        }

        public static void WriteFile(string pathConfigXml, Config config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            FileUtil.XmlSerialize(pathConfigXml, config);
        }
    }
}
