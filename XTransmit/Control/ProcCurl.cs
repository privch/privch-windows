using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using XTransmit.Utility;

namespace XTransmit.Control
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class ProcCurl
    {
        public static readonly string CurlExePath = $@"{App.DirectoryCurl}\{curl_exe_name}";

        /** curl-win64-mingw 7.68.0
         */
        private const string curl_exe_name = "xt-curl.exe"; // name "curl-x.exe" is for process control
        private const string curl_exe_process = "xt-curl";
        private const string curl_exe_md5 = "ED3506CA9DC52F0044035F7555EA8FF7";

        private const string libcurl_x64_dll_name = "libcurl-x64.dll";
        private const string libcurl_x64_dll_md5 = "C659B529429384AF7F9853F5E32F2B69";

        private const string curl_ca_bundle_crt_name = "curl-ca-bundle.crt";
        private const string curl_ca_bundle_crt_md5 = "C726AE88FD600AA26DF1D30F42B51FEC";

        public static void KillRunning()
        {
            // this list contains only this app's "curl" process
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
                    catch { }

                    process.Dispose();
                }
            }
        }

        public static bool Prepare()
        {
            // create directories and sub directories
            try
            {
                System.IO.Directory.CreateDirectory(App.DirectoryCurl);
            }
            catch
            {
                return false;
            }

            // check files
            object[][] checks =
            {
                new object[] { CurlExePath, curl_exe_md5, Properties.Resources.curl_exe_gz },
                new object[] { $@"{App.DirectoryCurl}\{libcurl_x64_dll_name}", libcurl_x64_dll_md5, Properties.Resources.libcurl_x64_dll_gz },
                new object[] { $@"{App.DirectoryCurl}\{curl_ca_bundle_crt_name}", curl_ca_bundle_crt_md5, Properties.Resources.curl_ca_bundle_crt_gz },
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
    }
}
