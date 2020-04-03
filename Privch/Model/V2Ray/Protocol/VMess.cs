using System.Collections.Generic;

namespace Privch.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
    public class VMess
    {
        public class VServer
        {
            public string address { get; set; } = string.Empty;

            public int port { get; set; } = 0;

            public VUser[] users { get; set; } = null;
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

            public int level { get; set; } = 0;
        }

        // best solution is suppress this analysis rule
        public VServer[] vnext { get; set; } = null;
    }
}
