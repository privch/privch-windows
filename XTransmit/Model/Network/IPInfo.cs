using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace XTransmit.Model.Network
{
    /**
     * Updated: 2019-08-04
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
                    Arguments = $"--data \"ip={ip}\" https://ipinfo.io",
                    WorkingDirectory = App.PathCurl,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };
            process.Start();

            string response = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();

            return ReadToObject(response);
        }

        private static IPInfo ReadToObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(IPInfo));

            IPInfo ipinfo = serializer.ReadObject(ms) as IPInfo;
            ms.Close();

            return ipinfo;
        }
    }
}
