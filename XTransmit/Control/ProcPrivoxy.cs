using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using XTransmit.Utility;

namespace XTransmit.Control
{
    /** Notes:
        privoxy[--help][--version][--no - daemon][--pidfile PIDFILE][--user USER
        [.GROUP]][--chroot][--pre - chroot - nslookup HOSTNAME][config_file]

        If no config_file is specified on the command line, Privoxy will look for a
        file named 'config' in the current directory (except Win32 which will look for
        'config.txt'). If no config_file is found, Privoxy will fail to start.
     */
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class ProcPrivoxy
    {
        public static readonly string PathPrivoxyExe = $@"{App.DirectoryPrivoxy}\{privoxy_exe_name}";
        private static Process process_privoxy = null;

        /** privoxy-windows 3.0.28
         */
        private const string privoxy_exe_name = "xt-privoxy.exe";
        private const string privoxy_exe_process = "xt-privoxy";
        private const string privoxy_exe_md5 = "3CF46F77B0917F08374E23EE59F7187F";

        private const string privoxy_config_txt_name = "privoxy-config.txt";
        private const string config_port_listen = "PORT-LISTEN";
        private const string config_port_forward_socks5 = "PORT-FORWARD-SOCKS5";

        public static void KillRunning()
        {
            // list contains all "privoxy" process
            Process[] list = Process.GetProcessesByName(privoxy_exe_process);
            if (list != null && list.Length > 0)
            {
                foreach (Process process in list)
                {
                    // kill app's privoxy process
                    if (process.MainModule.FileName == PathPrivoxyExe)
                    {
                        try
                        {
                            //process.CloseMainWindow();
                            process.Kill();
                            process.WaitForExit();
                        }
                        catch { }
                    }

                    process.Dispose();
                }
            }
        }

        public static bool Prepare()
        {
            // create directories and sub directories
            try
            {
                System.IO.Directory.CreateDirectory(App.DirectoryPrivoxy);
            }
            catch
            {
                return false;
            }

            // check files
            if (!FileUtil.CheckMD5(PathPrivoxyExe, privoxy_exe_md5))
            {
                if (!FileUtil.UncompressGZ(PathPrivoxyExe, Properties.Resources.privoxy_exe_gz))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Start(int portPrivoxy, int portShadowsocks)
        {
            string config_path = $@"{App.DirectoryPrivoxy}\{privoxy_config_txt_name}";
            string config_text = Properties.Resources.privoxy_config_txt;

            config_text = config_text.Replace(config_port_listen, portPrivoxy.ToString(CultureInfo.InvariantCulture));
            config_text = config_text.Replace(config_port_forward_socks5, portShadowsocks.ToString(CultureInfo.InvariantCulture));

            if (!FileUtil.WriteUTF8(config_path, config_text))
            {
                return false;
            }

            // process privoxy
            try
            {
                process_privoxy = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = PathPrivoxyExe,
                        Arguments = config_path,
                        WorkingDirectory = App.DirectoryPrivoxy,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        //LoadUserProfile = false,
                        //WindowStyle = ProcessWindowStyle.Hidden,
                    });
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static void Stop()
        {
            if (process_privoxy == null)
            {
                return;
            }

            try
            {
                //process_privoxy.CloseMainWindow();
                process_privoxy.Kill();
                process_privoxy.WaitForExit();
            }
            catch { }

            // it calls the Close method
            process_privoxy.Dispose();
        }
    }
}
