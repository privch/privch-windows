using System;
using System.Windows;
using XTransmit.Utility;
using XTransmit.ViewModel.Element;

namespace XTransmit.Model
{
    /**<summary>
     * UI Preference, Such as window position, window size, tab status.
     * </summary>
     */
    [Serializable]
    public class Preference
    {
        public bool IsWindowHomeVisible { get; set; }
        public string ContentDisplay { get; set; }

        public Placement WindowHome { get; set; }
        public Placement WindowSetting { get; set; }
        public Placement WindowAbout { get; set; }
        public Placement WindowServerConfig { get; set; }

        public Placement WindowCurl { get; set; }
        public Placement WindowCurlRunner { get; set; }
        public Placement WindowIPAddress { get; set; }
        public Placement WindowUserAgent { get; set; }

        /** construct to default values.
         */
        public Preference()
        {
            IsWindowHomeVisible = true;
            ContentDisplay = "";

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
                W = 0, // SizeToContent
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
                W = 0, // SizeToContent
                H = 0,
            };

            WindowCurl = new Placement
            {
                X = sw * 0.2,
                Y = sh * 0.2,
                W = sw * 0.6,
                H = sh * 0.6,
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
