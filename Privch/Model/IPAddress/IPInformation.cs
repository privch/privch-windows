using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Privch.Utility;

namespace Privch.Model.IPAddress
{
    [Serializable]
    public class IPInformation
    {
        #region Property
        public string IP { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Hostname { get; set; }
        public string Location { get; set; }
        public string Organization { get; set; }
        public string Timezone { get; set; }
        #endregion

        private static readonly HttpClientHandler httpHandler = new HttpClientHandler
        {
            //Proxy = new System.Net.WebProxy("127.0.0.1", SettingManager.Configuration.SystemProxyPort),
            UseProxy = false,
        };

        private static readonly HttpClient httpClient = new HttpClient(httpHandler, true)
        {
            Timeout = TimeSpan.FromMilliseconds(SettingManager.Configuration.TimeoutUpdateInfo),
        };

        // is it necessary ?
        public static void Dispose()
        {
            httpClient.Dispose();
            httpHandler.Dispose();
        }

        // Copy by serializer
        public IPInformation Copy() => (IPInformation)TextUtil.CopyBySerializer(this);

        /**<summary>
         * Retrieve data from https://ipinfo.io and read to a IPInfo object.
         * </summary>
         * TODO - UA, Proxy Parameter.
         */
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static IPInformation FromIPInfoIO(string ip)
        {
            Uri uri = new Uri("https://ipinfo.io/" + ip);

            //httpClient.DefaultRequestHeaders.Accept.Add(
            //    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string ipinfo;
            try
            {
                var response = httpClient.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                ipinfo = response.Content.ReadAsStringAsync().Result;
            }
            catch
            {
                return null;
            }

            if (TextUtil.JsonDeserialize(ipinfo, typeof(IPInfoIO)) is IPInfoIO ipinfoio)
            {
                return new IPInformation()
                {
                    IP = ipinfoio.ip,
                    City = ipinfoio.city,
                    Region = ipinfoio.region,
                    Country = ipinfoio.country,
                    Hostname = ipinfoio.hostname,
                    Location = ipinfoio.location,
                    Organization = ipinfoio.organization,
                    Timezone = ipinfoio.timezone,
                };
            }

            return null;
        }

        #region Equals
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
        #endregion
    }
}
