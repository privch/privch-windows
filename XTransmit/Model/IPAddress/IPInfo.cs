using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace XTransmit.Model.IPAddress
{
    [DataContract(Name = "IPInfoIO")]
    public class IPInfoIO : IExtensibleDataObject
    {
        private ExtensionDataObject extensionDataObjectValue;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionDataObjectValue;
            }
            set
            {
                extensionDataObjectValue = value;
            }
        }

        [DataMember(Name = "ip")]
        internal string ip = null;

        [DataMember(Name = "hostname")]
        internal string hostname = null;

        [DataMember(Name = "city")]
        internal string city = null;

        [DataMember(Name = "region")]
        internal string region = null;

        [DataMember(Name = "country")]
        internal string country = null;

        [DataMember(Name = "loc")]
        internal string location = null;

        [DataMember(Name = "org")]
        internal string organization = null;

        [DataMember(Name = "postal")]
        internal string postal = null;

        [DataMember(Name = "timezone")]
        internal string timezone = null;
    }

    [Serializable]
    public class IPInfo
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
        public IPInfo Copy() => (IPInfo)Utility.TextUtil.CopyBySerializer(this);

        /**<summary>
         * Fetch data from https://ipinfo.io and read to a IPInfo object.
         * TODO - UA, Proxy Parameter.
         * </summary>
         */
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static IPInfo Fetch(string ip)
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

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static IPInfo ReadToObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            IPInfoIO ipinfoio = null;
            MemoryStream msJson = new MemoryStream(Encoding.UTF8.GetBytes(json));

            try
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(IPInfoIO));
                ipinfoio = deserializer.ReadObject(msJson) as IPInfoIO;
            }
            catch
            {
                return null;
            }
            finally
            {
                msJson.Close(); // caused ca2202, why ? 
                msJson.Dispose();
            }

            return new IPInfo()
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

        /** Serializable ==================================================
         */
        public override int GetHashCode() => IP?.GetHashCode() ?? -1;

        public override bool Equals(object objectNew)
        {
            if (objectNew is IPInfo ipinfo)
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
