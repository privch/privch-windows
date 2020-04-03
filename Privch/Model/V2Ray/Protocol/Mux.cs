namespace Privch.Model.V2Ray.Protocol
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class Mux
    {
        public bool enabled { get; set; } = false;

        public int concurrency { get; set; } = 8; // 1-1024
    }
}
