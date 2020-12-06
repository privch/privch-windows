using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PrivCh.Model.V2Ray;
using PrivCh.Utility;

namespace PrivCh.Control
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    class ProcV2Ray
    {
        // can't use field here
        private static string V2RayExePath => $@"{App.DirectoryV2Ray}\{v2ray_exe_name}";
        private static Process process_v2ray;

        /** v2ray-core 4.22.1
         */
        private const string v2ctl_exe_name = "v2ctl.exe";
        private const string v2ctl_exe_md5 = "1446549EF5AC7BABEF3EDEF73E85E573";

        private const string v2ray_exe_name = "xt-v2ray.exe";
        private const string v2ray_exe_process = "xt-v2ray";
        private const string v2ray_exe_md5 = "DBD0F2E61ED67D945BCD23409D4B81B2";

        private const string v2ray_config_json_name = "v2ray-config.json";
        private const string config_listen_port = "PORT-LISTEN";
        private const string config_outbound = "PrivCh-Outbound";

        public static void KillRunning()
        {
            SystemUtil.KillProcess(v2ray_exe_process, V2RayExePath);
        }

        public static bool Prepare()
        {
            // create directories and sub directories
            try
            {
                System.IO.Directory.CreateDirectory(App.DirectoryV2Ray);
            }
            catch
            {
                return false;
            }

            // check files
            object[][] checks =
            {
                new object[] { $@"{App.DirectoryV2Ray}\{v2ctl_exe_name}", v2ctl_exe_md5, Properties.Resources.v2ctl_exe_gz },
                new object[] { V2RayExePath, v2ray_exe_md5, Properties.Resources.v2ray_exe_gz },
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

        public static bool Start(V2RayVMess server, int listen)
        {
            string config_path = $@"{App.DirectoryV2Ray}\{v2ray_config_json_name}";
            string config_text = Properties.Resources.v2ray_config_json;

            string outbound = V2RayVMess.ToJson(server);
            config_text = config_text.Replace(config_listen_port, listen.ToString(CultureInfo.InvariantCulture));
            config_text = config_text.Replace(config_outbound, outbound);

            if (!FileUtil.WriteUTF8(config_path, config_text))
            {
                return false;
            }

            try
            {
                process_v2ray = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = V2RayExePath,
                        Arguments = $@"-config {config_path}",
                        WorkingDirectory = App.DirectoryV2Ray,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        LoadUserProfile = false,
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
            if (process_v2ray == null)
            {
                return;
            }

            try
            {
                //process_privoxy.CloseMainWindow();
                process_v2ray.Kill();
                process_v2ray.WaitForExit();
            }
            catch { }

            // it calls the Close method
            process_v2ray.Dispose();
        }
    }
}
