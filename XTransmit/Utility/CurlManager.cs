using System;
using System.Diagnostics;
using System.Linq;

/**curl-7.65.3-win64-mingw 7.66.0
 * Updated: 2019-09-22
 */

namespace XTransmit.Utility
{
    public static class CurlManager
    {
        public static readonly string PathCurlExe = $@"{App.PathCurl}\{curl_exe_name}";

        private const string curl_exe_name = "curl-x.exe"; // name "curl-x.exe" is for process control
        private const string curl_exe_process = "curl-x";
        private const string curl_exe_md5 = "4db15511286782dc5ae8156e61482d35";

        private const string libcurl_x64_dll_name = "libcurl-x64.dll";
        private const string libcurl_x64_dll_md5 = "530d1f8002357f0335ed498d1c9e0ab1";

        private const string curl_ca_bundle_crt_name = "curl-ca-bundle.crt";
        private const string curl_ca_bundle_crt_md5 = "6c8779e5755d9dddf677bf7a52d035ce";

        public static bool Prepare()
        {
            // Creates all directories and subdirectories
            try { System.IO.Directory.CreateDirectory(App.PathCurl); }
            catch (Exception) { return false; }

            // Check binary files
            object[,] checks =
            {
                { PathCurlExe, curl_exe_md5, Properties.Resources.curl_exe_gz },
                { $@"{App.PathCurl}\{libcurl_x64_dll_name}", libcurl_x64_dll_md5, Properties.Resources.libcurl_x64_dll_gz },
                { $@"{App.PathCurl}\{curl_ca_bundle_crt_name}", curl_ca_bundle_crt_md5, Properties.Resources.curl_ca_bundle_crt_gz },
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
            // this list contain only this app's "curl" process
            Process[] list = Process.GetProcessesByName(curl_exe_process);
            if (list == null || list.Length == 0)
                return false;

            // kill app's curl-x process
            try
            {
                Process running = list.First(process => process.MainModule.FileName == PathCurlExe);
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
    }
}
