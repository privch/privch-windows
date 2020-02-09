namespace XTransmit.Model.V2Ray
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1812", Justification = "<Pending>")]
    internal class Outbound
    {
        public static readonly string[] Protocols = {
            "Blackhole",
            "Dokodemo-door",
            "Freedom",
            "HTTP",
            "MTProto",
            "Shadowsocks",
            "Socks",
            "VMess",
        };

        internal string tag = null;        
        internal string sendThrough = "0.0.0.0"; // not implement
        internal string protocol = null;        
        internal object settings = null; // protocol implement
        internal Transport.StreamSettings streamSettings = null;        
        internal string proxySettings = null; // not implement
        internal Mux mux = null;
    }
}
