using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace XTransmit.Model.IPAddress
{
    /**
     * Updated: 2019-09-28
     */
    public class IPInfo
    {
        public string ip;
        public string hostname;
        public string city;
        public string region;
        public string country;
        public string loc;
        public string org;
        public string postal;
        public string timezone;

        // Copy by serializer
        public IPInfo Copy() => (IPInfo)Utility.TextUtil.CopyBySerializer(this);

        /**<summary>
         * Fetch data from https://ipinfo.io and read to a IPInfo object.
         * </summary>
         */
        public static IPInfo Fetch(string ip)
        {
            // TODO - UA, Proxy Parameter
            // curl process
            Process process = new Process()
            {
                StartInfo =
                {
                    FileName = Utility.CurlManager.PathCurlExe,
                    Arguments = $"--header \"Accept: application/json\" ipinfo.io/{ip}",
                    WorkingDirectory = App.PathCurl,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };

            string response = null;
            try
            {
                process.Start();
                response = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch { }
            finally
            {
                process.Dispose();
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
            msJson.Close();
            msJson.Dispose();

            return ipinfo;
        }
    }
}
