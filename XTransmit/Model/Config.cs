using System;
using System.IO;
using System.Xml.Serialization;
using XTransmit.Model.Server;

namespace XTransmit.Model
{
    /**<summary>
     * Feature options. Such as SystemProxy, Privoxy, Shadowsocks.
     * Updated: 2019-08-02
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
        public static Config LoadFileOrDefault(string fileOptionXml)
        {
            Config config;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                FileStream fileStream = new FileStream(fileOptionXml, FileMode.Open);
                config = (Config)xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception)
            {
                config = new Config();
            }

            return config;
        }

        public static void WriteFile(string fileOptionXml, Config config)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                StreamWriter writer = new StreamWriter(fileOptionXml);
                xmlSerializer.Serialize(writer, config);
                writer.Close();
            }
            catch (Exception) { }
        }
    }
}
