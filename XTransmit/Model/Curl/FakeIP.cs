using System.Text.RegularExpressions;

namespace XTransmit.Model.Curl
{
    /** Affect http head only
     * Updated: 2019-09-28
     */
    class FakeIP
    {
        public const string Pattern = @"\[IP/\S+\]";
        public enum Method { Pick, Generate };

        public string Replace;
        public Method FakeMethod;

        /**
         * <summary>
         * [IP/Pick], [IP/Gen]
         * </summary>
         */
        public static FakeIP From(string input)
        {
            Match fakeMatch = Regex.Match(input, Pattern);
            if (!fakeMatch.Success)
            {
                return null;
            }

            string fakeMethod = fakeMatch.Value.Substring(4, fakeMatch.Value.Length - 5);
            if (string.IsNullOrWhiteSpace(fakeMethod))
            {
                return null;
            }

            Method method;
            if (fakeMethod == "Pick")
            {
                method = Method.Pick;
            }
            else if (fakeMethod == "Gen")
            {
                method = Method.Generate;
            }
            else
            {
                return null;
            }
            
            return new FakeIP()
            {
                Replace = fakeMatch.Value,
                FakeMethod = method
            };
        }
    }
}
