using System.Collections.Generic;

namespace PrivCh.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class VMess
    {
        public class VServer
        {
            public string address { get; set; } = string.Empty;

            public int port { get; set; }

            public VUser[] users { get; set; }
        }

        public class VUser
        {
            public static readonly List<string> Security = new List<string> {
                "aes-128-gcm",
                "chacha20-poly1305",
                "auto",
                "none",
            };

            public string id { get; set; } = string.Empty;

            public int alterId { get; set; } = 4; // recommand value is 4, default value is 0

            public string security { get; set; } = "auto";

            public int level { get; set; }
        }

        // best solution is suppress this analysis rule
        public VServer[] vnext { get; set; }
    }
}
