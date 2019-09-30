using System.Text.RegularExpressions;

namespace XTransmit.Model.Curl
{
    /** Proxy traffic
     * Updated: 2019-09-29
     */
    class FakeClient
    {
        public const string Pattern = @"\[Proxy/\S+\]";
        public enum Method { Socks5 };

        public string Replace;
        public Method FakeMethod;

        /**
         * <summary>
         * [Proxy/Socks5]
         * </summary>
         */
        public static FakeClient From(string input)
        {
            Match fakeMatch = Regex.Match(input, Pattern);
            if (!fakeMatch.Success)
            {
                return null;
            }

            string fakeMethod = fakeMatch.Value.Substring(7, fakeMatch.Value.Length - 8);
            if (string.IsNullOrWhiteSpace(fakeMethod))
            {
                return null;
            }

            Method method;
            if (fakeMethod == "Socks5")
            {
                method = Method.Socks5;
            }
            else
            {
                return null;
            }

            return new FakeClient()
            {
                Replace = fakeMatch.Value,
                FakeMethod = method,
            };
        }
    }
}
