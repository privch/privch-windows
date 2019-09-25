using System;
using System.Collections.Generic;
using System.Text;

namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-09-26
     */
    [Serializable]
    public class SiteProfile
    {
        // site info
        public string Title { get; set; }
        public string Website { get; set; }
        public string TimeUpdated { get; set; }

        // play settings
        public int PlayTimes { get; set; }
        public int DelayMin { get; set; }
        public int DelayMax { get; set; }

        // curl. 
        public bool IsReadResponse { get; set; }
        public List<CurlArgument> ArgumentList { get; set; } // List is hard to copy
        // TODO - Proxy auto config
        public string Socks5Proxy; // not used

        /**<summary>
         * Return a new object with all properties to default value.
         * When an object is constructed by the xml serializer via the parameterless constructor, 
         * The object properties value will be overwritten from the XML file.
         * </summary>
         */
        public static SiteProfile Default() => new SiteProfile
        {
            Title = "Title",
            Website = "Website",
            TimeUpdated = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),

            PlayTimes = 1,
            DelayMin = 0,
            DelayMax = 0,

            IsReadResponse = true,
            ArgumentList = new List<CurlArgument>(),
        };

        // copy object, use serializer
        public SiteProfile Copy() => (SiteProfile)Utility.TextUtil.CopyBySerializer(this);

        public string GetArguments()
        {
            StringBuilder sb = new StringBuilder("--silent");
            foreach (CurlArgument curlArgument in ArgumentList)
            {
                try
                {
                    string argument = curlArgument.Argument;
                    if (argument.StartsWith("-"))
                    {
                        sb.Append(' ').Append(argument);
                    }
                }
                catch { }

                try
                {
                    string value = curlArgument.Value;
                    if (value.Length > 2)
                    {
                        sb.Append(' ').Append(value);
                    }
                }
                catch { }
            }

            return sb.ToString();
        }
    }
}
