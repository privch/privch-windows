using System;
using System.Diagnostics;
using System.Linq;
using XTransmit.Model.Server;

/**
 * shadowsocks-libev-win-release-x86_64, 2019-07-23
 * 
 * Updated: 2019-08-02
 */

namespace XTransmit.Utility
{
    /** NOTE 
        -s <server_host>           Host name or IP address of your remote server.
        -p <server_port>           Port number of your remote server.
        -l <local_port>            Port number of your local server.
        -k <password>              Password of your remote server.
        -m <encrypt_method>        Encrypt method: rc4-md5,
                                   aes-128-gcm, aes-192-gcm, aes-256-gcm,
                                   aes-128-cfb, aes-192-cfb, aes-256-cfb,
                                   aes-128-ctr, aes-192-ctr, aes-256-ctr,
                                   camellia-128-cfb, camellia-192-cfb,
                                   camellia-256-cfb, bf-cfb,
                                   chacha20-ietf-poly1305,
                                   xchacha20-ietf-poly1305,
                                   salsa20, chacha20 and chacha20-ietf.
                                   The default cipher is chacha20-ietf-poly1305.

        [-a <user>]                Run as another user.
        [-f <pid_file>]            The file path to store pid.
        [-t <timeout>]             Socket timeout in seconds.
        [-c <config_file>]         The path to config file.
        [-n <number>]              Max number of open files.
        [-i <interface>]           Network interface to bind.
        [-b <local_address>]       Local address to bind.

        [-u]                       Enable UDP relay.
        [-U]                       Enable UDP relay and disable TCP relay.

        [--reuse-port]             Enable port reuse.
        [--fast-open]              Enable TCP fast open.
                                   with Linux kernel > 3.7.0.
        [--acl <acl_file>]         Path to ACL (Access Control List).
        [--mtu <MTU>]              MTU of your network interface.
        [--no-delay]               Enable TCP_NODELAY.
        [--key <key_in_base64>]    Key of your remote server.
        [--plugin <name>]          Enable SIP003 plugin. (Experimental)
        [--plugin-opts <options>]  Set SIP003 plugin options. (Experimental)

        [-v]                       Verbose mode.
        [-h, --help]               Print this message.
    */

    public static class SSManager
    {
        public static readonly string PathSSLocalExe = $@"{App.PathShadowsocks}\{ss_local_exe_name}";
        private static Process process_ss_local = null;

        private const string cygev_4_dll_name = "cygev-4.dll";
        private const string cygev_4_dll_md5 = "8778ace544923bfc2f57e9c14477de47";

        private const string cyggcc_s_seh_1_dll_name = "cyggcc_s-seh-1.dll";
        private const string cyggcc_s_seh_1_dll_md5 = "07124e6ff3cf0f67d3850fd9d764c64b";

        private const string cygmbedcrypto_3_dll_name = "cygmbedcrypto-3.dll";
        private const string cygmbedcrypto_3_dll_md5 = "fda3c9fc0b1e38fa9fae1651a94022ad";

        private const string cygpcre_1_dll_name = "cygpcre-1.dll";
        private const string cygpcre_1_dll_md5 = "255790aa072c536fc8cd46c97b133542";

        private const string cygsodium_23_dll_name = "cygsodium-23.dll";
        private const string cygsodium_23_dll_md5 = "6db350a38250dc74ba7ec1247d8402db";

        private const string cygwin1_dll_name = "cygwin1.dll";
        private const string cygwin1_dll_md5 = "42c5eb56ae8be10f34b53bef76caa24e";

        private const string ss_local_exe_name = "ss-local-x.exe"; // name ss-local-x.exe is for process control
        private const string ss_local_exe_process = "ss-local-x";
        private const string ss_local_exe_md5 = "a08004d58b653e23fa3bfde8e79e1c93";

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

        public static bool KillRunning()
        {
            // this list contain only this app's "ss" process
            Process[] list = Process.GetProcessesByName(ss_local_exe_process);
            if (list == null || list.Length == 0)
                return false;

            // kill app's ss-local-x process
            try
            {
                Process running = list.First(process => process.MainModule.FileName == PathSSLocalExe);
                if (running == null)
                    return false;

                running.CloseMainWindow();
                running.Kill();
                running.WaitForExit();

                running.Dispose();
                running = null;
                return true;
            }
            catch (Exception)
            {
                return false;
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
            catch (Exception)
            {
            }

            process_ss_local.Dispose();
            process_ss_local = null;
        }
    }
}
