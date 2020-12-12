using System.Runtime.Serialization;

namespace PrivCh.Model.IPAddress
{
    [DataContract(Name = "IpInfo")]
    public class IpInfo
    {
        [DataMember(Name = "ip")]
        internal string ip = null;

        [DataMember(Name = "hostname")]
        internal string hostname = null;

        [DataMember(Name = "city")]
        internal string city = null;

        [DataMember(Name = "region")]
        internal string region = null;

        [DataMember(Name = "country")]
        internal string country = null;

        [DataMember(Name = "loc")]
        internal string location = null;

        [DataMember(Name = "timezone")]
        internal string timezone = null;
    }
}
