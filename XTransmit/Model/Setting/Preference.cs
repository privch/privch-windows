using System;
using System.Windows;
using XTransmit.ViewModel.Element;

namespace XTransmit.Model.Setting
{
    /**<summary>
     * UI Preference, Such as window position, window size, tab status.
     * </summary>
     */
    [Serializable]
    public class Preference
    {
        public bool IsDarkTheme { get; set; }
        public bool IsWindowHomeVisible { get; set; }

        public string HomeContentDisplay { get; set; }
        public string NetworkAdapter { get; set; }

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
            IsDarkTheme = true;
            IsWindowHomeVisible = true;

            HomeContentDisplay = null;
            NetworkAdapter = null;

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
    }
}
