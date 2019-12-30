using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;

namespace XTransmit.Model.IPAddress
{
    [DataContract(Name = "IPInfoIO")]
    public class IPInfoIO : IExtensibleDataObject
    {
        private static readonly string sr_not_availabe = (string)Application.Current.FindResource("not_availabe");

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
        internal string ip = sr_not_availabe;

        [DataMember(Name = "hostname")]
        internal string hostname = sr_not_availabe;

        [DataMember(Name = "city")]
        internal string city = sr_not_availabe;

        [DataMember(Name = "region")]
        internal string region = sr_not_availabe;

        [DataMember(Name = "country")]
        internal string country = sr_not_availabe;

        [DataMember(Name = "loc")]
        internal string loc = sr_not_availabe;

        [DataMember(Name = "org")]
        internal string org = sr_not_availabe;

        [DataMember(Name = "postal")]
        internal string postal = sr_not_availabe;

        [DataMember(Name = "timezone")]
        internal string timezone = sr_not_availabe;
    }

    [Serializable]
    public class IPInfo
    {
        public string IP { get; set; }
        public string Hostname { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Loc { get; set; }
        public string Org { get; set; }
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
            catch { }
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
                Loc = ipinfoio.loc,
                Org = ipinfoio.org,
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
