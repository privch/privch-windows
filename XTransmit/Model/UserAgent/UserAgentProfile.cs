using System;

namespace XTransmit.Model.UserAgent
{
    /**
     * Updated: 2019-09-24
     */
    [Serializable]
    public class UserAgentProfile
    {
        public string os { get; set; } = "";
        public string client { get; set; } = "";
        public string value { get; set; } = "";

        /** Serializable ======================================================
         */
        public override int GetHashCode() => value.GetHashCode();
        public override bool Equals(object newUserAgent)
        {
            if (!string.IsNullOrEmpty(value) && newUserAgent is UserAgentProfile ua)
            {
                return (value.Equals(ua.value));
            }
            else
            {
                return false;
            }
        }
    }
}
