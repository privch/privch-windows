using System.Linq;
using PrivCh.Model.Setting;
using PrivCh.Utility;

namespace PrivCh.Model
{
    internal static class SettingManager
    {
        public static Config Configuration;
        public static Preference Appearance;

        //status
        public static BaseServer RemoteServer;
        public static bool IsServerPoolEnabled;

        private const string ServerTypeSS = "Shadowsocks";
        private const string ServerTypeV2Ray = "V2Ray";

        /**<summary>
         * Object is constructed by serializer with default values,
         * property (which also specified in the XML) value will be overwritten from the XML
         * </summary>
         */
        public static void LoadFileOrDefault(string pathConfigXml, string pathPrefXml)
        {
            // load configuration
            if (FileUtil.XmlDeserialize(pathConfigXml, typeof(Config)) is Config config)
            {
                Configuration = config;

                // restore status
                if (Configuration.RemoteServerType == ServerTypeSS)
                {
                    RemoteServer = ServerManager.ShadowsocksList.FirstOrDefault(
                    server => server.GetId() == config.RemoteServerID);
                }
                else if (Configuration.RemoteServerType == ServerTypeV2Ray)
                {
                    RemoteServer = ServerManager.V2RayList.FirstOrDefault(
                    server => server.GetId() == config.RemoteServerID);
                }
                else
                {
                    RemoteServer = null;
                }
            }
            else
            {
                Configuration = new Config();
            }

            // load appearance
            if (FileUtil.XmlDeserialize(pathPrefXml, typeof(Preference)) is Preference preference)
            {
                Appearance = preference;
            }
            else
            {
                Appearance = new Preference();
            }
        }

        public static void WriteFile(string pathConfigXml, string pathPrefXml)
        {
            // save status
            Configuration.RemoteServerID = RemoteServer?.GetId();
            if (RemoteServer is SS.Shadowsocks)
            {
                Configuration.RemoteServerType = ServerTypeSS;
            }
            else if (RemoteServer is V2Ray.V2RayVMess)
            {
                Configuration.RemoteServerType = ServerTypeV2Ray;
            }
            else
            {
                Configuration.RemoteServerType = null;
            }

            FileUtil.XmlSerialize(pathConfigXml, Configuration);
            FileUtil.XmlSerialize(pathPrefXml, Appearance);
        }
    }
}
