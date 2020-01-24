using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace XTransmit.Utility
{
    /** Notes:
        privoxy[--help][--version][--no - daemon][--pidfile PIDFILE][--user USER
        [.GROUP]][--chroot][--pre - chroot - nslookup HOSTNAME][config_file]

        If no config_file is specified on the command line, Privoxy will look for a
        file named 'config' in the current directory (except Win32 which will look for
        'config.txt'). If no config_file is found, Privoxy will fail to start.
     */
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public static class PrivoxyManager
    {
        public static readonly string PathPrivoxyExe = $@"{App.PathPrivoxy}\{privoxy_exe_name}";
        private static Process process_privoxy = null;

        /** privoxy-windows 3.0.28
         */
        private const string privoxy_exe_name = "privoxy.exe";
        private const string privoxy_exe_process = "privoxy";
        private const string privoxy_exe_md5 = "01286f3784dd8271db0e2f2a28040045";

        private const string privoxy_config_txt_name = "privoxy-config.txt";

        public static void KillRunning()
        {
            // list contains all "privoxy" process
            Process[] list = Process.GetProcessesByName(privoxy_exe_process);
            if (list != null && list.Length > 0)
            {
                foreach (Process process in list)
                {
                    // kill app's privoxy process
                    try
                    {
                        if (process.MainModule.FileName == PathPrivoxyExe)
                        {
                            process.CloseMainWindow();
                            process.Kill();
                            process.WaitForExit();
                        }
                    }
                    catch (Exception) { }

                    process.Dispose();
                }
            }
        }

        public static bool Prepare()
        {
            // create directories and sub directories
            try { System.IO.Directory.CreateDirectory(App.PathPrivoxy); }
            catch (Exception) { return false; }

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
            string config_path = $@"{App.PathPrivoxy}\{privoxy_config_txt_name}";
            string config_text = Properties.Resources.privoxy_config_txt;

            config_text = config_text.Replace("PORT_PRIVOXY", portPrivoxy.ToString(CultureInfo.InvariantCulture));
            config_text = config_text.Replace("PORT_SSLOCAL", portShadowsocks.ToString(CultureInfo.InvariantCulture));

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
                        WorkingDirectory = App.PathPrivoxy,
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
            catch (Exception) { }

            // it calls the Close method
            process_privoxy.Dispose();
        }
    }
}
