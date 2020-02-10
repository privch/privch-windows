namespace XTransmit.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1812", Justification = "<Pending>")]
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
    }
}
