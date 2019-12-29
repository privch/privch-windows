using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace XTransmit.Model.IPAddress
{
    [Serializable]
    public class IPInfo
    {
        public string ip { get; set; }
        public string hostname { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string loc { get; set; }
        public string org { get; set; }
        public string postal { get; set; }
        public string timezone { get; set; }

        // Copy by serializer
        public IPInfo Copy() => (IPInfo)Utility.TextUtil.CopyBySerializer(this);

        /**<summary>
         * Fetch data from https://ipinfo.io and read to a IPInfo object.
         * TODO - UA, Proxy Parameter.
         * </summary>
         */
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static IPInfo Fetch(string ip)
        {
            int timeout = App.GlobalConfig.IPInfoConnTimeout;

            // curl process
            Process process = null;
            string response = null;
            try
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = Utility.CurlManager.CurlExePath,
                        Arguments = $"--silent --connect-timeout {timeout} --header \"Accept: application/json\" ipinfo.io/{ip}",
                        WorkingDirectory = App.PathCurl,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    });

                response = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch { }
            finally
            {
                process?.Dispose();
            }

            return ReadToObject(response);
        }

        private static IPInfo ReadToObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            MemoryStream msJson = new MemoryStream(Encoding.UTF8.GetBytes(json));
            IPInfo ipinfo = new DataContractJsonSerializer(typeof(IPInfo)).ReadObject(msJson) as IPInfo;
            msJson.Close(); // caused ca2202, why ? 
            msJson.Dispose(); 

            return ipinfo;
        }

        /** Serializable ==================================================
         */
        public override int GetHashCode() => ip.GetHashCode();

        public override bool Equals(object objectNew)
        {
            if (objectNew is IPInfo ipinfo)
            {
                return ip == ipinfo.ip;
            }
            else
            {
                return false;
            }
        }
    }
}
