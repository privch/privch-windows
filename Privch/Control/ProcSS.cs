using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PrivCh.Model;
using PrivCh.Model.SS;
using PrivCh.Utility;

namespace PrivCh.Control
{
    internal static class ProcSS
    {
        // can't use field here
        private static string SSExePath => $@"{App.DirectoryShadowsocks}\{ss_local_exe_name}";

        /** shadowsocks-libev 3.3.5-mingw-x64
         */
        private const string libbloom_dll_name = "libbloom.dll";
        private const string libbloom_dll_md5 = "E0263B08EF7A64E8621F1584E094E3D6";

        private const string libcork_dll_name = "libcork.dll";
        private const string libcork_dll_md5 = "FB6CA972B6A5F24DF04D18B8C5DFE1CB";

        private const string libev_4_dll_name = "libev-4.dll";
        private const string libev_4_dll_md5 = "4B893C7C4AFFA24B6F2346DD68995F8F";

        private const string libgcc_s_seh_1_dll_name = "libgcc_s_seh-1.dll";
        private const string libgcc_s_seh_1_dll_md5 = "E760F496AC8726441DE778C7B2B50836";

        private const string libipset_dll_name = "libipset.dll";
        private const string libipset_dll_md5 = "34AA3399324C43D6AF8FA83DE8C4B58B";

        private const string libmbedcrypto_dll_name = "libmbedcrypto.dll";
        private const string libmbedcrypto_dll_md5 = "D0C101D8A58A991BF6DC2A75111DB026";

        private const string libpcre_1_dll_name = "libpcre-1.dll";
        private const string libpcre_1_dll_md5 = "16028C973F16F99F2503D93662B76E84";

        private const string libsodium_23_dll_name = "libsodium-23.dll";
        private const string libsodium_23_dll_md5 = "512D3839FC1E45F2B65A7C85E0E20E54";

        private const string libwinpthread_1_dll_name = "libwinpthread-1.dll";
        private const string libwinpthread_1_dll_md5 = "33AEDD85802056A8A89FEDE5882EB4EC";

        private const string ss_local_exe_name = "xt-ss-local.exe"; // name ss-local-x.exe is for process control
        private const string ss_local_exe_process = "xt-ss-local";
        private const string ss_local_exe_md5 = "ECDD8DFA1E0883A9E518B384630B97E6";

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
                new object[] { $@"{App.DirectoryShadowsocks}\{libbloom_dll_name}", libbloom_dll_md5, Properties.Resources.libbloom_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libcork_dll_name}", libcork_dll_md5, Properties.Resources.libcork_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libev_4_dll_name}", libev_4_dll_md5, Properties.Resources.libev_4_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libgcc_s_seh_1_dll_name}", libgcc_s_seh_1_dll_md5, Properties.Resources.libgcc_s_seh_1_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libipset_dll_name}", libipset_dll_md5, Properties.Resources.libipset_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libmbedcrypto_dll_name}", libmbedcrypto_dll_md5, Properties.Resources.libmbedcrypto_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libpcre_1_dll_name}", libpcre_1_dll_md5, Properties.Resources.libpcre_1_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libsodium_23_dll_name}", libsodium_23_dll_md5, Properties.Resources.libsodium_23_dll },
                new object[] { $@"{App.DirectoryShadowsocks}\{libwinpthread_1_dll_name}", libwinpthread_1_dll_md5, Properties.Resources.libwinpthread_1_dll },
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
