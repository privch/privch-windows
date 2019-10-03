using System.IO;
using System.Windows;
using XTransmit.Model;
using XTransmit.Utility;

namespace XTransmit
{
    /**TODO - English, Chinese language
     * TODO - App Analyze
     * TODO - Reset preference if environment has changed
     * TODO - Check memory leak, stream close, object dispose.
     * TODO - Display no data available
     * TODO - Add support for Remote Http Proxy, SSR, V2Ray ...
     * TODO - Auto search and add servers
     * TODO - Auto detect and remove invalid servers
     * 
     * NOTE
     * EventHandler name "_"
     * 
     * Updated: 2019-09-30
     */
    public partial class App : Application
    {
        public static string PathCurrent { get; private set; }
        public static string PathPrivoxy { get; private set; }
        public static string PathShadowsocks { get; private set; }
        public static string PathCurl { get; private set; }

        public static string FilePreferenceXml { get; private set; }
        public static string FileConfigXml { get; private set; }
        public static string FileIPAddressXml { get; private set; }
        public static string FileUserAgentXml { get; private set; }

        public static string FileServerXml { get; private set; }
        public static string FileCurlXml { get; private set; }

        public static Preference GlobalPreference { get; private set; }
        public static Config GlobalConfig { get; private set; }

        private View.TrayNotify.SystemTray NotifyIcon;

        public static void CloseMainWindow()
        {
            Current.MainWindow.Hide();
            Current.MainWindow.Close();
        }

        public static void ShowMainWindow()
        {
            if (Current.MainWindow.IsVisible)
            {
                if (Current.MainWindow.WindowState == WindowState.Minimized)
                    Current.MainWindow.WindowState = WindowState.Normal;

                Current.MainWindow.Activate();
            }
            else
            {
                Current.MainWindow.Show();
            }
        }

        public static void ShowNotify(string message)
        {
            View.WindowHome windowHome = (View.WindowHome)Current.MainWindow;
            windowHome.SendSnakebarMessage(message);
        }

        public static void UpdateProgress(int progress)
        {
            View.WindowHome windowHome = (View.WindowHome)Current.MainWindow;
            ViewModel.HomeVModel homeViewModel = (ViewModel.HomeVModel)windowHome.DataContext;
            homeViewModel.UpdateProgress(progress);
        }

        public static void LockTransmit(bool enable)
        {
            View.WindowHome windowHome = (View.WindowHome)Current.MainWindow;
            ViewModel.HomeVModel homeViewModel = (ViewModel.HomeVModel)windowHome.DataContext;
            homeViewModel.LockTransmitControl(enable);
        }

        public static void EnableTransmit(bool enable)
        {
            View.WindowHome windowHome = (View.WindowHome)Current.MainWindow;
            ViewModel.HomeVModel homeViewModel = (ViewModel.HomeVModel)windowHome.DataContext;
            homeViewModel.IsTransmitEnabled = enable;
        }

        public static void UpdateTransmitServer(Model.Server.ServerProfile serverProfile)
        {
            View.WindowHome windowHome = (View.WindowHome)Current.MainWindow;
            ViewModel.HomeVModel homeViewModel = (ViewModel.HomeVModel)windowHome.DataContext;
            homeViewModel.UpdateTransmitServer(serverProfile);
        }

        public static void AddServerByScanQRCode()
        {
            View.WindowHome windowHome = (View.WindowHome)Current.MainWindow;
            ViewModel.HomeVModel homeViewModel = (ViewModel.HomeVModel)windowHome.DataContext;
            homeViewModel.AddServerByScanQRCode();
        }

        /** Application ===============================================================================
         */
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // init directory
            PathCurrent = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            try { Directory.CreateDirectory($@"{PathCurrent}\datas"); }
            catch { return; }

            PathPrivoxy = $@"{PathCurrent}\binary\privoxy";
            PathShadowsocks = $@"{PathCurrent}\binary\shadowsocks";
            PathCurl = $@"{PathCurrent}\binary\curl";

            FilePreferenceXml = $@"{PathCurrent}\datas\Preference.xml";
            FileConfigXml = $@"{PathCurrent}\datas\Config.xml";
            FileIPAddressXml = $@"{PathCurrent}\datas\IPAddress.xml"; //china ip optimized
            FileUserAgentXml = $@"{PathCurrent}\datas\UserAgent.xml";

            FileServerXml = $@"{PathCurrent}\datas\Servers.xml";
            FileCurlXml = $@"{PathCurrent}\datas\Curl.xml";

            // init binaries
            PrivoxyManager.KillRunning();
            SSManager.KillRunning();
            CurlManager.KillRunning();
            if (!PrivoxyManager.Prepare() || !SSManager.Prepare() || !CurlManager.Prepare())
            {
                string app_name = (string)FindResource("app_name");
                string app_error_binary = (string)FindResource("app_error_binary");
                new View.DialogPrompt(app_name, app_error_binary).ShowDialog();

                Shutdown();
                return;
            }

            // load data
            GlobalPreference = Preference.LoadFileOrDefault(FilePreferenceXml);
            GlobalConfig = Config.LoadFileOrDefault(FileConfigXml);

            // notifyicon
            NotifyIcon = new View.TrayNotify.SystemTray();
            Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NotifyIcon.Dispose();

            /** if there were other proxy servers running they should set system proxy again
             */
            NativeMethods.DisableProxy();
            PrivoxyManager.Stop();
            SSManager.KillRunning(); // server pool

            Preference.WriteFile(FilePreferenceXml, GlobalPreference);
            Config.WriteFile(FileConfigXml, GlobalConfig);
        }

        // Something wrong happen, Unexpercted, Abnormally
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO - Handle exception safety
            //string app_name = (string)FindResource("app_name");
            //new View.DialogPrompt(app_name, e.Exception.Message).ShowDialog();
            Shutdown();
        }
    }
}
