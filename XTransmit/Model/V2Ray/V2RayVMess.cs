using System;
using System.Text;
using XTransmit.Utility;

namespace XTransmit.Model.V2Ray
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public class V2RayVMess : BaseServer
    {
        #region Properties(Serializable)
        public string Protocol { get; } = "VMess";

        public string Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string AlterId
        {
            get => alterId;
            set
            {
                alterId = value;
                OnPropertyChanged(nameof(AlterId));
            }
        }

        public string Network
        {
            get => network;
            set
            {
                network = value;
                OnPropertyChanged(nameof(Network));
            }
        }

        public string Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public string Host
        {
            get => host;
            set
            {
                host = value;
                OnPropertyChanged(nameof(Host));
            }
        }

        public string Tls
        {
            get => tls;
            set
            {
                tls = value;
                OnPropertyChanged(nameof(Tls));
            }
        }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public string Remarks
        {
            get => remarks;
            set
            {
                remarks = value;
                OnPropertyChanged(nameof(Remarks));
            }
        }
        #endregion

        // values 
        private string id;
        private string alterId;
        private string network;
        private string type;
        private string host;
        private string tls;
        private string path;
        private string remarks;


        public V2RayVMess Copy()
        {
            return (V2RayVMess)TextUtil.CopyBySerializer(this);
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = $"{HostAddress} - {HostPort}";
        }


        #region Fectory
        // start with vmess://
        public static V2RayVMess FromQRCode(string vmessBase64)
        {
            if (string.IsNullOrWhiteSpace(vmessBase64) ||
                !vmessBase64.StartsWith("vmess://", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // decode base64 vmess uri
            string vmessJson;
            try
            {
                byte[] data = Convert.FromBase64String(vmessBase64.Substring(8));
                vmessJson = Encoding.UTF8.GetString(data);
            }
            catch
            {
                return null;
            }

            // deserialize to VMessUri
            if (TextUtil.JsonDeserialize(vmessJson, typeof(VMessUri)) is VMessUri vmessUri)
            {
                return new V2RayVMess
                {
                    HostAddress = vmessUri.address,
                    HostPort = vmessUri.port,

                    Id = vmessUri.id,
                    AlterId = vmessUri.alterId,
                    Network = vmessUri.network,
                    Type = vmessUri.type,
                    Host = vmessUri.host,
                    Tls = vmessUri.tls,
                    Path = vmessUri.path,
                    Remarks = vmessUri.ps,
                };
            }

            return null;
        }

        // create form standard vmess protocol json string: "vnext": [...]
        public static V2RayVMess FromVMessProtocol(string json)
        {
            if (TextUtil.JsonDeserialize(json, typeof(Protocol.VMess)) is Protocol.VMess vmess)
            {
                if (vmess.vnext != null && vmess.vnext.Length > 0)
                {
                    return new V2RayVMess
                    {
                        HostAddress = vmess.vnext[0].address,
                        HostPort = vmess.vnext[0].port,
                        FriendlyName = $"{vmess.vnext[0].address} - {vmess.vnext[0].port}",
                    };
                }
            }

            return null;
        }
        #endregion
    }
}
