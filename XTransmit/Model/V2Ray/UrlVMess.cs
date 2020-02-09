using System.Runtime.Serialization;

namespace XTransmit.Model.V2Ray
{
    [DataContract(Name = "URL-VMess")]
    internal class UrlVMess : IExtensibleDataObject
    {
        private ExtensionDataObject extensionDataObjectValue;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionDataObjectValue;
            }
            set
            {
                extensionDataObjectValue = value;
            }
        }

        [DataMember(Name = "ps")]
        internal string ps = null;

        [DataMember(Name = "add")]
        internal string addres = null;

        [DataMember(Name = "port")]
        internal int port = -1;

        [DataMember(Name = "id")]
        internal string id = null;

        [DataMember(Name = "aid")]
        internal string alterId = null;

        [DataMember(Name = "net")]
        internal string netwrok = null;

        [DataMember(Name = "type")]
        internal string type = null;

        [DataMember(Name = "host")]
        internal string host = null;

        [DataMember(Name = "tls")]
        internal string tls = null;
    }
}
