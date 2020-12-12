using System.Collections.Generic;

namespace PrivCh.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public static class Transport
    {
        public class StreamSettings
        {
            public static readonly List<string> Network = new List<string> {
                "tcp",
                "kcp",
                "ws",
                "http",
                "domainsocket",
                "quic",
            };

            public static readonly List<string> Security = new List<string> {
                "none",
                "tls",
            };

            public string network { get; set; } = "tcp";

            public string security { get; set; } = "none";

            public TLS tlsSettings { get; set; }

            public string tcpSettings { get; set; }  // not implement

            public string kcpSettings { get; set; } // not implement

            public WebSocket wsSettings { get; set; }

            public VHTTP httpSettings { get; set; }

            public DomainSocket dsSettings { get; set; }

            public string quicSettings { get; set; }  // Quick UDP Internet Connection. not implement

            public Sockopt sockopt { get; set; }
        }

        public class TLS
        {
            public string serverName { get; set; } = ""; // auto set

            public string[] alpn { get; set; } = { "http/1.1" };

            public bool allowInsecure { get; set; }

            public bool allowInsecureCiphers { get; set; }

            public bool disableSystemRoot { get; set; }

            public Certificate[] certificates { get; set; }
        }

        public class WebSocket
        {
            public string path { get; set; } = "/";

            public Dictionary<string, string> headers { get; set; } = null;
        }

        public class VHTTP
        {
            public string[] host { get; set; }

            public string path { get; set; } = "/";
        }

        public class DomainSocket
        {
            public string path { get; set; }  // path to ds file
        }

        public class Sockopt
        {
            public static readonly List<string> Tproxy = new List<string> {
                "redirect",
                "tproxy",
                "off"
            };

            public int mark { get; set; }  // linux SO_MARK

            public bool tcpFastOpen { get; set; }

            public string tproxy { get; set; } = "off";
        }

        public class Certificate
        {
            public static readonly List<string> Useage = new List<string> {
                "encipherment",
                "verify",
                "issue",
            };

            public string usage { get; set; } = "encipherment";

            // certificateFile or certificate, choose one
            public string certificateFile { get; set; } = ""; // path to certificate.crt. 
            public string[] certificate { get; set; } // content of certificate. 

            // keyFile or key, choose one
            public string keyFile { get; set; } = ""; // path to key.key
            public string[] key { get; set; } // content of key
        }
    }
}
