using System.Text.RegularExpressions;

namespace XTransmit.Model.Curl
{
    /** 
     * Replace the UA info in http header
     */
    class FakeUA
    {
        public const string Pattern = @"\[UA\]";
        public string Replace;

        /**
         * <summary>
         * [UA]. Case sensitive
         * </summary>
         */
        public static FakeUA From(string input)
        {
            Match fakeMatch = Regex.Match(input, Pattern);
            if (!fakeMatch.Success)
            {
                return null;
            }

            return new FakeUA()
            {
                Replace = fakeMatch.Value,
            };
        }
    }
}
