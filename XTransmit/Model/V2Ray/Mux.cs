using System;

namespace XTransmit.Model.V2Ray
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1812", Justification = "<Pending>")]
    internal class Mux
    {
        public bool enabled = false;
        public int concurrency = 8; // 1-1024
    }
}
