using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace XTransmit.Model.V2Ray
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1812", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal class VMess
    {
        public class VServer
        {
            public string address = string.Empty;
            public int port = 0;
            public VUser[] users = null;
        }

        public class VUser
        {
            public static readonly string[] Security = {
                "aes-128-gcm",
                "chacha20-poly1305",
                "auto",
                "none",
            };

            public string id = string.Empty;
            public int alterId = 4; // recommand value is 4, default value is 0
            public string security = "auto";
            public int level = 0;
        }

        public VServer[] vnext = null;


        public static VServer[] ServerFromJson(string json)
        {
            VServer[] servers = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return servers;
            }

            MemoryStream msJson = new MemoryStream(Encoding.UTF8.GetBytes(json));

            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VServer[]));
                servers = serializer.ReadObject(msJson) as VServer[];
            }
            catch { }

            msJson.Close(); // caused ca2202, why ? 
            msJson.Dispose();

            return servers;
        }

        public static string ServerToJson(VServer[] servers)
        {
            string json = null;
            MemoryStream msJson = new MemoryStream();
            StreamReader sreader = new StreamReader(msJson);

            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VServer[]));
                serializer.WriteObject(msJson, servers);

                msJson.Position = 0;
                json = sreader.ReadToEnd();
            }
            catch { }

            sreader.Close();
            msJson.Close();

            return json;
        }
    }
}
