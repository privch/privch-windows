using System.Text.RegularExpressions;

namespace XTransmit.Model.Curl
{
    /** 
     * Affect http header only
     */
    class FakeIP
    {
        public const string Pattern = @"\[IP/\S+\]";
        public enum Method { Pick, Gen };

        public string Replace;
        public Method FakeMethod;


        /**
         * <summary>
         * [IP/Pick], [IP/Gen]. Case sensitive
         * </summary>
         */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static FakeIP From(string input)
        {
            Match fakeMatch = Regex.Match(input, Pattern);
            if (!fakeMatch.Success)
            {
                return null;
            }

            string fakeMethod;
            try
            {
                fakeMethod = fakeMatch.Value.Substring(4, fakeMatch.Value.Length - 5);
            }
            catch
            {
                return null;
            }

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
                method = Method.Gen;
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
