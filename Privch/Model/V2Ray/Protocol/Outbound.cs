using System.Collections.Generic;

namespace Privch.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class Outbound
    {
        public static readonly List<string> Protocols = new List<string> {
            "Blackhole",
            "Dokodemo-door",
            "Freedom",
            "HTTP",
            "MTProto",
            "Shadowsocks",
            "Socks",
            "VMess",
        };

        public string tag { get; set; } = null;

        public string sendThrough { get; set; } = "0.0.0.0"; // not implement

        public string protocol { get; set; } = null;

        public VMess settings { get; set; } = null; // protocol implement

        public Transport.StreamSettings streamSettings { get; set; } = null;

        public string proxySettings { get; set; } = null; // not implement

        public Mux mux { get; set; } = null;
    }
}
