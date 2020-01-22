using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace XTransmit.Model.Curl
{
    [Serializable]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
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
        public List<CurlArgument> ArgumentList { get; private set; }


        /**<summary>
         * Return a new object with all properties to default value.
         * When an object is constructed by the xml serializer via the parameterless constructor, 
         * The object properties value will be overwritten from the XML file.
         * </summary>
         */
        public SiteProfile()
        {
            Title = "Title";
            Website = "Website";
            TimeUpdated = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss", CultureInfo.InvariantCulture);

            PlayTimes = 1;
            DelayMin = 0; //no delay
            DelayMax = 0; 

            IsReadResponse = true;
            ArgumentList = new List<CurlArgument>();
        }

        // copy object by serializer
        public SiteProfile Copy() => (SiteProfile)Utility.TextUtil.CopyBySerializer(this);

        public string GetArguments()
        {
            StringBuilder sb = new StringBuilder("--silent");
            foreach (CurlArgument curlArgument in ArgumentList)
            {
                try
                {
                    string argument = curlArgument.Argument;
                    if (argument.StartsWith("-", StringComparison.Ordinal))
                    {
                        sb.Append(' ').Append(argument);
                    }
                }
                catch { }

                try
                {
                    string value = curlArgument.Value;
                    if (value.Length > 0) // such as timeout setting
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
