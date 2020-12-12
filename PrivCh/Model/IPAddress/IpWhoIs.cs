using System.Runtime.Serialization;

namespace PrivCh.Model.IPAddress
{
    [DataContract(Name = "IpWhoIs")]
    public class IpWhoIs
    {
        //public String ip;
        //public String success;
        //public String type;
        //public String continent;
        //public String continent_code;

        [DataMember(Name = "country")]
        public string country;

        //public String country_code;
        //public String country_flag;
        //public String country_capital;
        //public String country_phone;
        //public String country_neighbours;

        [DataMember(Name = "region")]
        public string region;

        [DataMember(Name = "city")]
        public string city;

        //public String latitude;
        //public String longitude;
        //public String asn;
        //public String org;
        //public String isp;
        //public String timezone;
        //public String timezone_name;
        //public String timezone_dstOffset;
        //public String timezone_gmtOffset;
        //public String timezone_gmt;
        //public String currency;
        //public String currency_code;
        //public String currency_symbol;
        //public String currency_rates;
        //public String currency_plural;
        //public String completed_requests;
    }
}
