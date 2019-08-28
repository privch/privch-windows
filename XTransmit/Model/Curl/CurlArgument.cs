using System;

namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-08-02
     */
    [Serializable]
    public class CurlArgument
    {
        public string Argument
        {
            get { return vArgument; }
            set { vArgument = value.Trim(); }
        }

        public string Value
        {
            get { return vValue; }
            set { vValue = value.Trim(); }
        }

        private string vArgument;
        private string vValue;
    }
}
