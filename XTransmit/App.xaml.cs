using MaterialDesignThemes.Wpf;
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
     * TODO - Use Task instead BackgroundWorker
     * TODO - Move the X-CURL feature to new process
     * 
     * NOTE
     * Compares to DataGrid, ListView comes with auto width, double click, row sort and application command problems
     * EventHandler name use "_"
     */
    public partial class App : Application
    {
        public static string Name { get; private set; }

        public static string DirectoryApplication { get; private set; }
        public static string DirectoryPrivoxy { get; private set; }
        public static string DirectoryShadowsocks { get; private set; }
        public static string DirectoryCurl { get; private set; }

        public static string FileApplication { get; private set; }
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

            // to avoid loading WindowHome on startup fails
            StartupUri = new System.Uri("View/WindowShutdown.xaml", System.UriKind.Relative);

            // single instance
            if (IsProcessExist())
            {
                Shutdown();
                return;
            }

            // init directory
            Name = (string)Current.FindResource("app_name");

            FileApplication = System.Reflection.Assembly.GetEntryAssembly().Location;
            DirectoryApplication = Path.GetDirectoryName(FileApplication);
            
            try
            {
                Directory.CreateDirectory($@"{DirectoryApplication}\{dirData}");
            }
            catch
            {
                string title = Name;
                string message = (string)FindResource("app_init_fail");
                new View.DialogPrompt(title, message).ShowDialog();

                Shutdown();
                return;
            }

            DirectoryPrivoxy = $@"{DirectoryApplication}\{dirBin}\privoxy";
            DirectoryShadowsocks = $@"{DirectoryApplication}\{dirBin}\shadowsocks";
            DirectoryCurl = $@"{DirectoryApplication}\{dirBin}\curl";

            FilePreferenceXml = $@"{DirectoryApplication}\{dirData}\Preference.xml";
            FileConfigXml = $@"{DirectoryApplication}\{dirData}\Config.xml";
            FileIPAddressXml = $@"{DirectoryApplication}\{dirData}\IPAddress.xml"; //china ip optimized
            FileUserAgentXml = $@"{DirectoryApplication}\{dirData}\UserAgent.xml";

            FileServerXml = $@"{DirectoryApplication}\{dirData}\Servers.xml";
            FileCurlXml = $@"{DirectoryApplication}\{dirData}\Curl.xml";

            // initialize binaries
            PrivoxyManager.KillRunning();
            SSManager.KillRunning();
            CurlManager.KillRunning();
            if (!PrivoxyManager.Prepare() || !SSManager.Prepare() || !CurlManager.Prepare())
            {
                string title = Name;
                string message = (string)FindResource("app_init_fail");
                new View.DialogPrompt(title, message).ShowDialog();

                Shutdown();
                return;
            }

            // load data
            PreferenceManager.LoadFileOrDefault(FilePreferenceXml);
            ConfigManager.LoadFileOrDefault(FileConfigXml);

            // initialize transmit
            if (!TransmitCtrl.StartServer())
            {
                string title = Name;
                string message = (string)FindResource("app_service_fail");
                new View.DialogPrompt(title, message).ShowDialog();

                Shutdown();
                return;
            }

            TransmitCtrl.EnableTransmit(ConfigManager.Global.IsTransmitEnabled);

            // initialize interface and theme
            InterfaceCtrl.Initialize();
            InterfaceCtrl.ModifyTheme(theme => theme.SetBaseTheme(
                PreferenceManager.Global.IsDarkTheme ? Theme.Dark : Theme.Light));

            // done
            StartupUri = new System.Uri("View/WindowHome.xaml", System.UriKind.Relative);
            Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            TransmitCtrl.StopServer();
            InterfaceCtrl.Uninit();

            // if there were other proxy servers running they should set system proxy again
            _ = NativeMethods.DisableProxy();
            SSManager.KillRunning();
            CurlManager.KillRunning();

            PreferenceManager.WriteFile(FilePreferenceXml);
            ConfigManager.WriteFile(FileConfigXml);

            // fix autorun status. reduce startup time
            if (ConfigManager.Global.IsAutorun)
            {
                SystemUtil.CreateUserStartupShortcut();
            }
            else
            {
                SystemUtil.DeleteUserStartupShortcuts();
            }
        }

        // Something wrong happen, Unexpercted, Abnormally (Not set)
        private void Application_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Start another process to send feedback for user 
            Shutdown();
        }
    }
}
