using System.Collections.Generic;

namespace XTransmit.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
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

            public TLS tlsSettings { get; set; } = null;

            public string tcpSettings { get; set; } = null; // not implement

            public string kcpSettings { get; set; } = null; // not implement

            public WebSocket wsSettings { get; set; } = null;

            public HTTP httpSettings { get; set; } = null;

            public DomainSocket dsSettings { get; set; } = null;

            public string quicSettings { get; set; } = null; // Quick UDP Internet Connection. not implement

            public Sockopt sockopt { get; set; } = null;
        }

        public class TLS
        {
            public string serverName { get; set; } = ""; // auto set

            public string[] alpn { get; set; } = { "http/1.1" };

            public bool allowInsecure { get; set; } = false;

            public bool allowInsecureCiphers { get; set; } = false;

            public bool disableSystemRoot { get; set; } = false;

            public Certificate[] certificates { get; set; } = null;
        }

        public class WebSocket
        {
            public string path { get; set; } = "/";

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
            public System.Collections.Generic.Dictionary<string, string> headers { get; set; } = null;
        }

        public class HTTP
        {
            public string[] host { get; set; } = null;

            public string path { get; set; } = "/";
        }

        public class DomainSocket
        {
            public string path { get; set; } = null; // path to ds file
        }

        public class Sockopt
        {
            public static readonly List<string> Tproxy = new List<string> {
                "redirect",
                "tproxy",
                "off"
            };

            public int mark { get; set; } = 0; // linux SO_MARK

            public bool tcpFastOpen { get; set; } = false;

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
            public string[] certificate { get; set; } = null; // content of certificate. 

            // keyFile or key, choose one
            public string keyFile { get; set; } = ""; // path to key.key
            public string[] key { get; set; } = null; // content of key
        }
    }
}
