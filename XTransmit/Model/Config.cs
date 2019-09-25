using System;
using System.IO;
using System.Xml.Serialization;
using XTransmit.Model.Server;

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
        public static Config LoadFileOrDefault(string fileOptionXml)
        {
            Config config;
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(fileOptionXml, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                config = (Config)xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception)
            {
                config = new Config();
            }
            finally
            {
                fileStream?.Dispose();
            }

            return config;
        }

        public static void WriteFile(string fileOptionXml, Config config)
        {
            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(fileOptionXml);
                new XmlSerializer(typeof(Config)).Serialize(streamWriter, config);
                streamWriter.Close();
            }
            catch (Exception) { }
            finally
            {
                streamWriter?.Dispose();
            }
        }
    }
}
