using System.Runtime.Serialization;

namespace Privch.Model.V2Ray
{
    [DataContract(Name = "VMessUri")]
    public class VMessUri
    {
        [DataMember(Name = "ps")]
        internal string ps = null;

        [DataMember(Name = "add")]
        internal string address = null;

        [DataMember(Name = "port")]
        internal int port = -1;

        [DataMember(Name = "id")]
        internal string id = null;

        [DataMember(Name = "aid")]
        internal int alterId = 0;

        [DataMember(Name = "net")]
        internal string network = null;

        [DataMember(Name = "type")]
        internal string type = null;

        [DataMember(Name = "host")]
        internal string host = null;

        [DataMember(Name = "tls")]
        internal string tls = null;

        [DataMember(Name = "path")]
        internal string path = null;
    }
}
