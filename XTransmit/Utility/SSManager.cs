using System;
using System.Diagnostics;
using System.Linq;
using XTransmit.Model.Server;

/**
 * shadowsocks-libev-win-x86_64, 2019-09-22
 * shadowsocks-libev 3.3.1
 * 
 * Updated: 2019-09-22
 */

namespace XTransmit.Utility
{
    public static class SSManager
    {
        public static readonly string PathSSLocalExe = $@"{App.PathShadowsocks}\{ss_local_exe_name}";
        private static Process process_ss_local = null;

        private const string cygev_4_dll_name = "cygev-4.dll";
        private const string cygev_4_dll_md5 = "1d4ab5325fe69fd662ab1b9af8c03145";

        private const string cyggcc_s_seh_1_dll_name = "cyggcc_s-seh-1.dll";
        private const string cyggcc_s_seh_1_dll_md5 = "b84b92d9ddb884a0af4b2e11e4224f45";

        private const string cygmbedcrypto_3_dll_name = "cygmbedcrypto-3.dll";
        private const string cygmbedcrypto_3_dll_md5 = "c9d2f4a389f86d30fcbd94de90849546";

        private const string cygpcre_1_dll_name = "cygpcre-1.dll";
        private const string cygpcre_1_dll_md5 = "ef24e5503e42d0f7b7ef7c8dfe298769";

        private const string cygsodium_23_dll_name = "cygsodium-23.dll";
        private const string cygsodium_23_dll_md5 = "1ecdffa32acffaf104e45c6c4de5a430";

        private const string cygwin1_dll_name = "cygwin1.dll";
        private const string cygwin1_dll_md5 = "42c5eb56ae8be10f34b53bef76caa24e";

        private const string ss_local_exe_name = "ss-local-x.exe"; // name ss-local-x.exe is for process control
        private const string ss_local_exe_process = "ss-local-x";
        private const string ss_local_exe_md5 = "cd5a05ed703aaa8b17a463ca23a4d828";

        public static bool Prepare()
        {
            // Creates all directories and subdirectories
            try { System.IO.Directory.CreateDirectory(App.PathShadowsocks); }
            catch (Exception) { return false; }

            // Check binary files
            object[,] checks =
            {
                { $@"{App.PathShadowsocks}\{cygev_4_dll_name}", cygev_4_dll_md5, Properties.Resources.cygev_4_dll_gz },
                { $@"{App.PathShadowsocks}\{cyggcc_s_seh_1_dll_name}", cyggcc_s_seh_1_dll_md5 , Properties.Resources.cyggcc_s_seh_1_dll_gz },
                { $@"{App.PathShadowsocks}\{cygmbedcrypto_3_dll_name}", cygmbedcrypto_3_dll_md5, Properties.Resources.cygmbedcrypto_3_dll_gz },
                { $@"{App.PathShadowsocks}\{cygpcre_1_dll_name}", cygpcre_1_dll_md5 , Properties.Resources.cygpcre_1_dll_gz },
                { $@"{App.PathShadowsocks}\{cygsodium_23_dll_name}", cygsodium_23_dll_md5 , Properties.Resources.cygsodium_23_dll_gz },
                { $@"{App.PathShadowsocks}\{cygwin1_dll_name}", cygwin1_dll_md5 , Properties.Resources.cygwin1_dll_gz },
                { PathSSLocalExe, ss_local_exe_md5, Properties.Resources.ss_local_exe_gz },
            };

            int length = checks.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                if (!FileUtil.CheckMD5((string)checks[i, 0], (string)checks[i, 1]))
                    FileUtil.UncompressGZ((string)checks[i, 0], (byte[])checks[i, 2]);
            }

            return true;
        }

        public static void KillRunning()
        {
            // this list contain only this app's "ss" process
            Process[] list = Process.GetProcessesByName(ss_local_exe_process);
            if (list != null && list.Length > 0)
            {
                // kill app's ss-local-x process
                try
                {
                    Process running = list.First(process => process.MainModule.FileName == PathSSLocalExe);
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

        public static bool Start(ServerProfile server, int portListen)
        {
            string arguments = $"-s {server.vHostIP} -p {server.vPort} -l {portListen} -k {server.vPassword} -m {server.vEncrypt} -t {server.vTimeout}";

            process_ss_local = new Process()
            {
                StartInfo =
                {
                    FileName = PathSSLocalExe,
                    Arguments = arguments,
                    WorkingDirectory = App.PathShadowsocks,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    LoadUserProfile = false,
                },
            };

            process_ss_local.Start();
            return true;
        }

        public static void Exit()
        {
            if (process_ss_local == null)
                return;

            try
            {
                process_ss_local.CloseMainWindow();
                process_ss_local.Kill();
                process_ss_local.WaitForExit();
            }
            catch (Exception) { }

            /**The Dispose method calls Close
             * https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.close
             */
            process_ss_local.Dispose();
        }
    }
}
