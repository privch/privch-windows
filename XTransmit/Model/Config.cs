using System;
using XTransmit.Model.Server;
using XTransmit.Utility;

namespace XTransmit.Model
{
    /**<summary>
     * Feature options. Such as SystemProxy, Privoxy, Shadowsocks.
     * Updated: 2019-09-24
     * </summary> 
     */
    [Serializable]
    public class Config
    {
        // transmit
        public bool IsTransmitEnabled;
        public int SystemProxyPort;
        public int GlobalSocks5Port;
        public ServerProfile RemoteServer;

        // defaults 
        public int ConnectionTimeouts;
        public int PingTimeouts;
        public string NetworkAdapter;

        public Config()
        {
            IsTransmitEnabled = false;

            SystemProxyPort = 0;
            GlobalSocks5Port = 0;
            RemoteServer = null;

            ConnectionTimeouts = 3;
            PingTimeouts = 1;
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
            FileUtil.XmlSerialize(pathConfigXml, config);
        }
    }
}
