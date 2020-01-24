using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using XTransmit.Control;
using XTransmit.Model;
using XTransmit.Utility;

namespace XTransmit
{
    /**
     * TODO - English, Chinese language
     * TODO - Check memory leak, stream close, object dispose.
     * TODO - Add support for Remote Http Proxy, SSR, V2Ray ...
     * TODO - Auto search and add servers
     * TODO - Auto detect and remove invalid servers
     * TODO - Autorun, Add a shortcut to the user Startup menu
     * TODO - Optimize readonly DataGrids, Use ListView, ListBox instead
     * 
     * NOTE
     * EventHandler name use "_"
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


        public static void CloseMainWindow()
        {
            PreferenceManager.Global.IsWindowHomeVisible = Current.MainWindow.IsVisible;
            Current.MainWindow.Hide();
            Current.MainWindow.Close();
        }

        public static void ShowMainWindow()
        {
            if (Current.MainWindow.IsVisible)
            {
                if (Current.MainWindow.WindowState == WindowState.Minimized)
                {
                    Current.MainWindow.WindowState = WindowState.Normal;
                }

                Current.MainWindow.Activate();
            }
            else
            {
                Current.MainWindow.Show();
            }
        }

        /** Application ===============================================================================
         */
        private bool IsProcessExist()
        {
            Process[] list = Process.GetProcessesByName("XTransmit");
            if (list != null && list.Length > 1)
            {
                foreach (Process process in list)
                {
                    process.Dispose();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string dirData = "data";
            string dirBin = "binary";

            // to avoid loading WindowHome on startup exceptions
            StartupUri = new System.Uri("View/WindowShutdown.xaml", System.UriKind.Relative);

            // single instance
            if (IsProcessExist())
            {
                Shutdown();
                return;
            }

            // init directory
            PathCurrent = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            try { Directory.CreateDirectory($@"{PathCurrent}\{dirData}"); }
            catch
            {
                Shutdown();
                return;
            }

            PathPrivoxy = $@"{PathCurrent}\{dirBin}\privoxy";
            PathShadowsocks = $@"{PathCurrent}\{dirBin}\shadowsocks";
            PathCurl = $@"{PathCurrent}\{dirBin}\curl";

            FilePreferenceXml = $@"{PathCurrent}\{dirData}\Preference.xml";
            FileConfigXml = $@"{PathCurrent}\{dirData}\Config.xml";
            FileIPAddressXml = $@"{PathCurrent}\{dirData}\IPAddress.xml"; //china ip optimized
            FileUserAgentXml = $@"{PathCurrent}\{dirData}\UserAgent.xml";

            FileServerXml = $@"{PathCurrent}\{dirData}\Servers.xml";
            FileCurlXml = $@"{PathCurrent}\{dirData}\Curl.xml";

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
            PreferenceManager.LoadFileOrDefault(FilePreferenceXml);
            ConfigManager.LoadFileOrDefault(FileConfigXml);

            // TODO - Alert and exit if fail
            InterfaceCtrl.Initialize();
            TransmitCtrl.StartServer();

            StartupUri = new System.Uri("View/WindowHome.xaml", System.UriKind.Relative);
            Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            InterfaceCtrl.Uninit();

            /** if there were other proxy servers running they should set system proxy again
             */
            _ = NativeMethods.DisableProxy();
            TransmitCtrl.StopServer();
            SSManager.KillRunning(); // server pool
            CurlManager.KillRunning();

            PreferenceManager.WriteFile(FilePreferenceXml);
            ConfigManager.WriteFile(FileConfigXml);
        }

        // Something wrong happen, Unexpercted, Abnormally
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO - Startup another process for user to send feedback
            //new View.DialogPrompt(app_name, e.Exception.Message).ShowDialog();
            Shutdown();
        }
    }
}
