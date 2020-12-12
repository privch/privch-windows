using System;
using System.Net.Http;

using PrivCh.Utility;

namespace PrivCh.Model.IPAddress
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

        public static IPInformation FromIpWhoIs(string ip)
        {
            Uri uri = new Uri("http://ipwhois.app/json/" + ip + "?objects=country,region,city");

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string result;
            try
            {
                var response = httpClient.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                result = response.Content.ReadAsStringAsync().Result;
            }
            catch
            {
                return null;
            }

            if (TextUtil.JsonDeserialize(result, typeof(IpWhoIs)) is IpWhoIs ipwhois)
            {
                return new IPInformation()
                {
                    IP = ip,
                    City = ipwhois.city,
                    Region = ipwhois.region,
                    Country = ipwhois.country,
                };
            }

            return null;
        }

        /**<summary>
         * Retrieve data from https://ipinfo.io and read to a IPInfo object.
         * </summary>
         * TODO - UA, Proxy Parameter.
         */
        public static IPInformation FromIpInfo(string ip)
        {
            Uri uri = new Uri("https://ipinfo.io/" + ip);

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string result;
            try
            {
                var response = httpClient.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                result = response.Content.ReadAsStringAsync().Result;
            }
            catch
            {
                return null;
            }

            if (TextUtil.JsonDeserialize(result, typeof(IpInfo)) is IpInfo ipinfo)
            {
                return new IPInformation()
                {
                    IP = ipinfo.ip,
                    City = ipinfo.city,
                    Region = ipinfo.region,
                    Country = ipinfo.country,
                    Hostname = ipinfo.hostname,
                    Location = ipinfo.location,
                    Timezone = ipinfo.timezone,
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
