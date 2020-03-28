﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using XTransmit.Model;
using XTransmit.Model.SS;
using XTransmit.Utility;

namespace XTransmit.Control
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class ProcSS
    {
        // can't use field here
        private static string SSExePath => $@"{App.DirectoryShadowsocks}\{ss_local_exe_name}";

        /** shadowsocks-libev 3.3.4
         */
        private const string cygev_4_dll_name = "cygev-4.dll";
        private const string cygev_4_dll_md5 = "0F6CE5CF3FE78F6B730155E4CE254185";

        private const string cyggcc_s_seh_1_dll_name = "cyggcc_s-seh-1.dll";
        private const string cyggcc_s_seh_1_dll_md5 = "0CB45DABA5809E2A67C9062637792E04";

        private const string cygmbedcrypto_3_dll_name = "cygmbedcrypto-3.dll";
        private const string cygmbedcrypto_3_dll_md5 = "48B2B0FD2CBA64784C487D329D010A81";

        private const string cygpcre_1_dll_name = "cygpcre-1.dll";
        private const string cygpcre_1_dll_md5 = "1020C690FDB824BF5D17C4FFC68ED71A";

        private const string cygsodium_23_dll_name = "cygsodium-23.dll";
        private const string cygsodium_23_dll_md5 = "DAD70135850DA1B8013A499AF9E16B2C";

        private const string cygwin1_dll_name = "cygwin1.dll";
        private const string cygwin1_dll_md5 = "476090DABDE7721FFA2BFD1C011DC6DE";

        private const string ss_local_exe_name = "xt-ss-local.exe"; // name ss-local-x.exe is for process control
        private const string ss_local_exe_process = "xt-ss-local";
        private const string ss_local_exe_md5 = "CB9B8D4C913304A531C07C65734CE53F";

        public static void KillRunning()
        {
            SystemUtil.KillProcess(ss_local_exe_process, SSExePath);
        }

        public static bool Prepare()
        {
            // create directories and sub directories
            try
            {
                System.IO.Directory.CreateDirectory(App.DirectoryShadowsocks);
            }
            catch
            {
                return false;
            }

            // check files
            object[][] checks =
            {
                new object[] { $@"{App.DirectoryShadowsocks}\{cygev_4_dll_name}", cygev_4_dll_md5, Properties.Resources.cygev_4_dll_gz },
                new object[] { $@"{App.DirectoryShadowsocks}\{cyggcc_s_seh_1_dll_name}", cyggcc_s_seh_1_dll_md5, Properties.Resources.cyggcc_s_seh_1_dll_gz },
                new object[] { $@"{App.DirectoryShadowsocks}\{cygmbedcrypto_3_dll_name}", cygmbedcrypto_3_dll_md5, Properties.Resources.cygmbedcrypto_3_dll_gz },
                new object[] { $@"{App.DirectoryShadowsocks}\{cygpcre_1_dll_name}", cygpcre_1_dll_md5, Properties.Resources.cygpcre_1_dll_gz },
                new object[] { $@"{App.DirectoryShadowsocks}\{cygsodium_23_dll_name}", cygsodium_23_dll_md5, Properties.Resources.cygsodium_23_dll_gz },
                new object[] { $@"{App.DirectoryShadowsocks}\{cygwin1_dll_name}", cygwin1_dll_md5, Properties.Resources.cygwin1_dll_gz },
                new object[] { SSExePath, ss_local_exe_md5, Properties.Resources.ss_local_exe_gz },
            };

            int length = checks.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                if (!FileUtil.CheckMD5((string)checks[i][0], (string)checks[i][1]))
                {
                    if (!FileUtil.UncompressGZ((string)checks[i][0], (byte[])checks[i][2]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static Process Execute(Shadowsocks server, int listen)
        {
            int timeout = SettingManager.Configuration.TimeoutShadowsocks;
            string arguments = $"-s {server.HostAddress} -p {server.HostPort} -l {listen} -k {server.Password} -m {server.Encrypt} -t {timeout}";

            Process process = null;
            try
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = SSExePath,
                        Arguments = arguments,
                        WorkingDirectory = App.DirectoryShadowsocks,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        LoadUserProfile = false,
                    });

                return process;
            }
            catch
            {
                // it is correct
                process?.Dispose();
            }

            return null;
        }

        public static void Exit(Process process)
        {
            try
            {
                process.Kill();
                process.WaitForExit();
            }
            catch { }
            finally
            {
                /** The dispose method calls the Close method
                 * https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.close
                 */
                process?.Dispose();
            }
        }
    }
}
