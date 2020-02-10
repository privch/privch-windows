using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using XTransmit.Control;
using XTransmit.Utility;

namespace XTransmit.Model.IPAddress
{
    [Serializable]
    public class IPInformation
    {
        public string IP { get; set; }
        public string Hostname { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Location { get; set; }
        public string Organization { get; set; }
        public string Postal { get; set; }
        public string Timezone { get; set; }

        // Copy by serializer
        public IPInformation Copy() => (IPInformation)Utility.TextUtil.CopyBySerializer(this);

        /**<summary>
         * Fetch data from https://ipinfo.io and read to a IPInfo object.
         * </summary>
         * TODO - UA, Proxy Parameter.
         */
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static IPInformation Fetch(string ip)
        {
            int timeout = ConfigManager.Global.IPInfoConnTimeout;

            // curl process
            Process process = null;
            string response = null;
            try
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = ProcCurl.CurlExePath,
                        Arguments = $"--silent --connect-timeout {timeout} --header \"Accept: application/json\" ipinfo.io/{ip}",
                        WorkingDirectory = App.DirectoryCurl,
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

            if (TextUtil.JsonDeserialize(response, typeof(IPInfoIO)) is IPInfoIO ipinfoio)
            {
                return new IPInformation()
                {
                    IP = ipinfoio.ip,
                    Hostname = ipinfoio.hostname,
                    City = ipinfoio.city,
                    Region = ipinfoio.region,
                    Country = ipinfoio.country,
                    Location = ipinfoio.location,
                    Organization = ipinfoio.organization,
                    Postal = ipinfoio.postal,
                    Timezone = ipinfoio.timezone,
                };
            }

            return null;
        }

        /** Serializable ==================================================
         */
        public override int GetHashCode() => IP?.GetHashCode() ?? -1;

        public override bool Equals(object objectNew)
        {
            if (objectNew is IPInformation ipinfo)
            {
                return IP == ipinfo.IP;
            }
            else
            {
                return false;
            }
        }
    }
}
