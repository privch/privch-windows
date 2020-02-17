using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
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
        public static async System.Threading.Tasks.Task<IPInformation> Fetch(string ip)
        {
            Uri uri = new Uri("https://ipinfo.io/" + ip);

            HttpClientHandler httpHandler = new HttpClientHandler
            {
                //Proxy = new System.Net.WebProxy("127.0.0.1", SettingManager.Configuration.SystemProxyPort),
                UseProxy = false,
            };

            HttpClient httpClient = new HttpClient(httpHandler, true)
            {
                Timeout = TimeSpan.FromMilliseconds(SettingManager.Configuration.TimeoutFetchInfo),
            };
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string ipinfo = null;
            try
            {
                var response = await httpClient.GetAsync(uri).ConfigureAwait(true);
                response.EnsureSuccessStatusCode();
                ipinfo = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            }
            catch
            {
                return null;
            }
            finally
            {
                httpClient.Dispose();
                httpHandler.Dispose();
            }

            if (TextUtil.JsonDeserialize(ipinfo, typeof(IPInfoIO)) is IPInfoIO ipinfoio)
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
