using System;
using System.Windows;
using XTransmit.Utility;
using XTransmit.ViewModel.Control;

namespace XTransmit.Model
{
    /**<summary>
     * UI Preference, Such as window position and size.
     * Updated: 2019-09-24
     * </summary>
     */
    [Serializable]
    public class Preference
    {
        public string ContentDisplay;

        public Placement WindowHome;
        public Placement WindowSetting;
        public Placement WindowAbout;
        public Placement WindowServerConfig;

        public Placement WindowCurlRunner;
        public Placement WindowIPAddress;
        public Placement WindowUserAgent;

        /** construct to default values.
         */
        public Preference()
        {
            double sw = SystemParameters.PrimaryScreenWidth;
            double sh = SystemParameters.PrimaryScreenHeight;

            WindowHome = new Placement
            {
                X = sw * 0.2,
                Y = sh * 0.2,
                W = sw * 0.6,
                H = sh * 0.6,
            };

            WindowSetting = new Placement
            {
                X = sw * 0.2,
                Y = sh * 0.2,
                W = 0, // SiteToContent
                H = 0,
            };

            WindowAbout = new Placement
            {
                X = sw * 0.4,
                Y = sh * 0.4,
                W = 0, // SizeToContent
                H = 0,
            };

            WindowServerConfig = new Placement
            {
                X = sw * 0.2,
                Y = sh * 0.2,
                W = 0, // SiteToContent
                H = 0,
            };

            WindowCurlRunner = new Placement
            {
                X = sw * 0.2,
                Y = sh * 0.2,
                W = sw * 0.6,
                H = sh * 0.6,
            };

            WindowIPAddress = new Placement
            {
                X = sw * 0.2,
                Y = sh * 0.2,
                W = sw * 0.6,
                H = sh * 0.6,
            };

            WindowUserAgent = new Placement
            {
                X = sw * 0.2,
                Y = sh * 0.2,
                W = sw * 0.6,
                H = sh * 0.6,
            };

            ContentDisplay = "";
        }


        /**<summary>
         * Object is constructed by serializer with default values,
         * property (which also specified in the XML) value will be overwritten from the XML
         * </summary>
         */
        public static Preference LoadFileOrDefault(string pathPrefXml)
        {
            if (FileUtil.XmlDeserialize(pathPrefXml, typeof(Preference)) is Preference preference)
            {
                return preference;
            }
            else
            {
                return new Preference();
            }
        }

        public static void WriteFile(string pathPrefXml, Preference preference)
        {
            FileUtil.XmlSerialize(pathPrefXml, preference);
        }
    }
}
