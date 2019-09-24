using System;
using System.Diagnostics;
using System.Linq;

/**
 * Updated: 2019-08-02
 */

namespace XTransmit.Utility
{
    /** Notes:
        privoxy[--help][--version][--no - daemon][--pidfile PIDFILE][--user USER
        [.GROUP]][--chroot][--pre - chroot - nslookup HOSTNAME][config_file]

        If no config_file is specified on the command line, Privoxy will look for a
        file named 'config' in the current directory (except Win32 which will look for
        'config.txt'). If no config_file is found, Privoxy will fail to start.
     */

    public static class PrivoxyManager
    {
        public static readonly string PathPrivoxyExe = $@"{App.PathPrivoxy}\{privoxy_exe_name}";
        private static Process process_privoxy = null;

        private const string privoxy_exe_name = "privoxy.exe";
        private const string privoxy_exe_process = "privoxy";
        private const string privoxy_exe_md5 = "bc77d427634503f3ee7603bf009d7e32";

        private const string privoxy_config_txt_name = "privoxy-config.txt";

        public static bool Prepare()
        {
            // Creates all directories and subdirectories
            try { System.IO.Directory.CreateDirectory(App.PathPrivoxy); }
            catch (Exception) { return false; }

            // Check binary files
            if (!FileUtil.CheckMD5(PathPrivoxyExe, privoxy_exe_md5))
                FileUtil.UncompressGZ(PathPrivoxyExe, Properties.Resources.privoxy_exe_gz);

            return true;
        }

        public static void KillRunning()
        {
            // list contains all "privoxy" process
            Process[] list = Process.GetProcessesByName(privoxy_exe_process);
            if (list != null && list.Length > 0)
            {
                // kill app's privoxy process
                try
                {
                    Process running = list.First(process => process.MainModule.FileName == PathPrivoxyExe);
                    if (running != null)
                    {
                        running.CloseMainWindow();
                        running.Kill();
                        running.WaitForExit();
                    }
                }
                catch (Exception) { }

                foreach (Process proc in list)
                {
                    proc.Dispose();
                }
            }
        }

        public static bool Start(int portPrivoxy, int portShadowsocks)
        {
            string config = Properties.Resources.privoxy_config_txt;
            config = config.Replace("PORT_PRIVOXY", portPrivoxy.ToString());
            config = config.Replace("PORT_SSLOCAL", portShadowsocks.ToString());

            if (!FileUtil.WriteUTF8($@"{App.PathPrivoxy}\{privoxy_config_txt_name}", config))
                return false;

            // process privoxy
            process_privoxy = new Process()
            {
                StartInfo =
                {
                    FileName = PathPrivoxyExe,
                    Arguments = $@"{App.PathPrivoxy}\{privoxy_config_txt_name}",
                    WorkingDirectory = App.PathPrivoxy,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    LoadUserProfile = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                }
            };

            process_privoxy.Start();
            return true;
        }

        public static void Exit()
        {
            if (process_privoxy == null)
                return;

            try
            {
                process_privoxy.CloseMainWindow();
                process_privoxy.Kill();
                process_privoxy.WaitForExit();
            }
            catch (Exception)
            {
            }

            process_privoxy.Dispose();
        }
    }
}
