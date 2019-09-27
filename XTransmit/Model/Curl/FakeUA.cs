using System.Text.RegularExpressions;

namespace XTransmit.Model.Curl
{
    /** Define the UA info in http head
     * Updated: 2019-09-28
     */
    class FakeUA
    {
        public const string Pattern = @"\[UA\]";
        public string Replace;

        /**
         * <summary>
         * [UA]
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
