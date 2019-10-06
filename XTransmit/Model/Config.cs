using System;
using XTransmit.Model.Server;
using XTransmit.Utility;

namespace XTransmit.Model
{
    /**<summary>
     * Feature options. Such as SystemProxy, Privoxy, Shadowsocks.
     * Updated: 2019-10-02
     * </summary> 
     */
    [Serializable]
    public class Config
    {
        // transmit
        public bool IsTransmitEnabled;
        public bool IsServerPoolEnabled;
        public int SystemProxyPort;
        public int GlobalSocks5Port;
        public ServerProfile RemoteServer;

        // timeouts 
        public int SSTimeout;
        public int IPInfoConnTimeout;
        public int ResponseConnTimeout; //not used
        public int PingTimeout; //ms

        public string NetworkAdapter;

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
            PingTimeout = 1200;
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
