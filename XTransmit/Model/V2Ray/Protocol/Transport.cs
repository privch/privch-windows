namespace XTransmit.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1812", Justification = "<Pending>")]
    internal class Transport
    {
        public class StreamSettings
        {
            public static readonly string[] Network = {
                "tcp",
                "kcp",
                "ws",
                "http",
                "domainsocket",
                "quic",
            };

            public static readonly string[] Security = {
                "none",
                "tls",
            };

            public string network = "tcp";
            public string security = "none";
            public TLS tlsSettings = null;
            public string tcpSettings = null; // not implement
            public string kcpSettings = null; // not implement
            public WebSocket wsSettings = null;
            public HTTP httpSettings = null;
            public DomainSocket dsSettings = null;
            public string quicSettings = null; // Quick UDP Internet Connection. not implement
            public Sockopt sockopt = null;
        }


        public class TLS
        {
            public string serverName = ""; // auto set
            public string[] alpn = { "http/1.1" };
            public bool allowInsecure = false;
            public bool allowInsecureCiphers = false;
            public bool disableSystemRoot = false;
            public Certificate[] certificates = null;
        }

        public class WebSocket
        {
            public string path = "/";
            public System.Collections.Generic.Dictionary<string, string> headers = null;
        }

        public class HTTP
        {
            public string[] host = null;
            public string path = "/";
        }

        public class DomainSocket
        {
            public string path = null; // path to ds file
        }

        public class Sockopt
        {
            public static readonly string[] Tproxy = {
                "redirect",
                "tproxy",
                "off"
            };

            public int mark = 0; // linux SO_MARK
            public bool tcpFastOpen = false;
            public string tproxy = "off";
        }

        public class Certificate
        {
            public static readonly string[] Useage = {
                "encipherment",
                "verify",
                "issue",
            };

            public string usage = "encipherment";

            // certificateFile or certificate, choose one
            public string certificateFile = ""; // path to certificate.crt. 
            public string[] certificate = null; // content of certificate. 

            // keyFile or key, choose one
            public string keyFile = ""; // path to key.key
            public string[] key = null; // content of key
        }
    }
}
