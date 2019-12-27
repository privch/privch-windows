using System;
using System.Diagnostics;

/**
 * curl-win64-mingw 7.67.0
 */
namespace XTransmit.Utility
{
    public static class CurlManager
    {
        public static string CurlExePath => $@"{App.PathCurl}\{curl_exe_name}";

        private const string curl_exe_name = "curl-x.exe"; // name "curl-x.exe" is for process control
        private const string curl_exe_process = "curl-x";
        private const string curl_exe_md5 = "190d43949bbd8e8ed59cb3f408523010";

        private const string libcurl_x64_dll_name = "libcurl-x64.dll";
        private const string libcurl_x64_dll_md5 = "4444374adc31bf391a5d4b11a57aa4ef";

        private const string curl_ca_bundle_crt_name = "curl-ca-bundle.crt";
        private const string curl_ca_bundle_crt_md5 = "3231fbcbbb54c2963dc37f7224f127ff";

        public static void KillRunning()
        {
            // this list contain only this app's "curl" process
            Process[] list = Process.GetProcessesByName(curl_exe_process);
            if (list != null && list.Length > 0)
            {
                foreach (Process process in list)
                {
                    // kill app's curl-x process
                    try
                    {
                        if (process.MainModule.FileName == CurlExePath)
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
            // Creates all directories and subdirectories
            try { System.IO.Directory.CreateDirectory(App.PathCurl); }
            catch (Exception) { return false; }

            // Check binary files
            object[,] checks =
            {
                { CurlExePath, curl_exe_md5, Properties.Resources.curl_exe_gz },
                { $@"{App.PathCurl}\{libcurl_x64_dll_name}", libcurl_x64_dll_md5, Properties.Resources.libcurl_x64_dll_gz },
                { $@"{App.PathCurl}\{curl_ca_bundle_crt_name}", curl_ca_bundle_crt_md5, Properties.Resources.curl_ca_bundle_crt_gz },
            };

            int length = checks.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                if (!FileUtil.CheckMD5((string)checks[i, 0], (string)checks[i, 1]))
                {
                    if(!FileUtil.UncompressGZ((string)checks[i, 0], (byte[])checks[i, 2]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
