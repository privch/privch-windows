using System;
using System.ComponentModel;

namespace XTransmit.Model.Server
{
    [Serializable]
    public class V2Ray
    {
        /** Protocol VMess 
         */
        internal class VMessUser
        {
            public static readonly string[] Security = {
                "aes-128-gcm",
                "chacha20-poly1305",
                "auto",
                "none",
            };

            public string id;
            public int alterId = 4; // recommand value is 4, default value is 0
            public string security = "auto";
            public int level;
        }

        internal class VMessServer
        {
            public string address;
            public int port;
            public VMessUser[] users;
        }

        class VMess
        {
            public VMessServer[] vnext;
        }


        /** Mux
         */
        internal class Mux
        {
            public bool enabled = false;
            public int concurrency = 8; // 1-1024
        }


        /** StreamSettings
         */
        internal class TLS
        {
            public string serverName = ""; // auto set
            public string[] alpn = { "http/1.1" };
            public bool allowInsecure = false;
            public bool allowInsecureCiphers = false;
            public bool disableSystemRoot = false;
            public Certificate[] certificates;
        }

        internal class WebSocket
        {
            public string path = "/";
            public System.Collections.Generic.Dictionary<string, string> headers = null;
        }

        internal class HTTP
        {
            public string[] host;
            public string path = "/";
        }

        internal class DomainSocket
        {
            public string path; // path to ds file
        }

        internal class Sockopt
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

        internal class Certificate
        {
            public static readonly string[] Useage = {
                "encipherment",
                "verify",
                "issue",
            };

            public string usage = "encipherment";

            // certificateFile or certificate, choose one
            public string certificateFile = ""; // path to certificate.crt. 
            public string[] certificate; // content of certificate. 

            // keyFile or key, choose one
            public string keyFile = ""; // path to key.key
            public string[] key; // content of key
        }
        
        internal class StreamSettings
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


        /** V2Ray Outbound 
         */
        public string tag;
        public string protocol;
        public object settings;


        /** INotifyPropertyChanged =========================================
         */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
