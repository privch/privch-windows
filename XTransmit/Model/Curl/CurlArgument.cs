using System;

namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-09-30
     */
    [Serializable]
    public class CurlArgument
    {
        public string Argument
        {
            get { return argument_option; }
            set { argument_option = value.Trim(); }
        }

        public string Value
        {
            get { return argument_value; }
            set { argument_value = value.Trim(); }
        }

        private string argument_option;
        private string argument_value;
    }
}
