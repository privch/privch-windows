using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using XTransmit.Model;
using XTransmit.Utility;
using XTransmit.ViewModel.Element;

namespace XTransmit
{
    /**
     * TODO - English, Chinese language
     * TODO - Check memory leak, stream close, object dispose.
     * TODO - Add support for Remote Http Proxy, SSR, V2Ray ...
     * TODO - Auto search and add servers
     * TODO - Auto detect and remove invalid servers
     * TODO - Icon for the status of server pool mode
     * TODO - Autorun, Add a shortcut to the user Startup menu
     * TODO - Optimize readonly DataGrids, Use ListView, ListBox instead
     * TODO - May need to add a controller
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

        public static Preference GlobalPreference { get; private set; }
        public static Config GlobalConfig { get; private set; }

        public static View.TrayNotify.SystemTray NotifyIcon { get; private set; }

        // controller ==================================================
        public static void UpdateTransmitLock()
        {
            // WindowHome is null on shutdown. NotifyIcon updates status at menu popup
            if (Current.MainWindow is View.WindowHome windowHome
                && windowHome.DataContext is ViewModel.HomeVModel homeViewModel)
            {
                homeViewModel.UpdateLockTransmit();
            }
        }

        public static void AddHomeProgress(TaskView task)
        {
            if (Current.MainWindow is View.WindowHome windowHome
                && windowHome.DataContext is ViewModel.HomeVModel homeViewModel)
            {
                homeViewModel.AddTask(task);
            }
        }

        public static void RemoveHomeProgress(TaskView task)
        {
            if (Current.MainWindow is View.WindowHome windowHome
                && windowHome.DataContext is ViewModel.HomeVModel homeViewModel)
            {
                homeViewModel.RemoveTask(task);
            }
        }

        public static void UpdateHomeTransmitStatue()
        {
            if (Current.MainWindow is View.WindowHome windowHome
                && windowHome.DataContext is ViewModel.HomeVModel homeViewModel)
            {
                homeViewModel.UpdateTransmitStatus();
            }
        }

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

        public static void ShowHomeNotify(string message)
        {
            View.WindowHome windowHome = (View.WindowHome)Current.MainWindow;
            windowHome.SendSnakebarMessage(message);
        }

        public static void EnableTransmit(bool enable)
        {
            if (enable)
            {
                if (NativeMethods.EnableProxy($"127.0.0.1:{GlobalConfig.SystemProxyPort}", NativeMethods.Bypass) != 0)
                {
                    GlobalConfig.IsTransmitEnabled = true;
                }
            }
            else
            {
                if (NativeMethods.DisableProxy() != 0)
                {
                    GlobalConfig.IsTransmitEnabled = false;
                }
            }

            UpdateHomeTransmitStatue();
            NotifyIcon.SwitchIcon(GlobalConfig.IsTransmitEnabled);
        }

        public static void ChangeTransmitServer(Model.Server.ServerProfile serverProfile)
        {
            TransmitControl.ChangeTransmitServer(serverProfile);
            UpdateHomeTransmitStatue();
        }

        public static void AddServerByScanQRCode()
        {
            if (Current.MainWindow is View.WindowHome windowHome
                && windowHome.DataContext is ViewModel.HomeVModel homeViewModel)
            {
                homeViewModel.AddServerByScanQRCode();
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
            GlobalPreference = Preference.LoadFileOrDefault(FilePreferenceXml);
            GlobalConfig = Config.LoadFileOrDefault(FileConfigXml);

            // TODO - Alert and exit if fail
            TransmitControl.StartServer();

            // notifyicon
            NotifyIcon = new View.TrayNotify.SystemTray();
            StartupUri = new System.Uri("View/WindowHome.xaml", System.UriKind.Relative);
            Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NotifyIcon.Dispose();

            /** if there were other proxy servers running they should set system proxy again
             */
            _ = NativeMethods.DisableProxy();
            TransmitControl.StopServer();
            SSManager.KillRunning(); // server pool
            CurlManager.KillRunning();

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
