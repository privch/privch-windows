using System;
using XTransmit.Utility;

namespace XTransmit.Model.V2Ray
{
    [Serializable]
    public class V2RayServer : BaseServer
    {
        #region Properties(Serializable)
        public string Protocol
        {
            get => protocol;
            set
            {
                protocol = value;
                OnPropertyChanged(nameof(Protocol));
            }
        }

        public string Json
        {
            get => json;
            set
            {
                json = value;
                OnPropertyChanged(nameof(Json));
            }
        }
        #endregion

        // values 
        private string protocol;
        private string json;
        private object jsonObject;
        

        public V2RayServer Copy()
        {
            return (V2RayServer)TextUtil.CopyBySerializer(this);
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = $"{HostAddress} - {HostPort}";
        }


        #region Fectory
        public static V2RayServer Create(string protocol, string json)
        {
            if (string.Equals(protocol, "vmess", StringComparison.OrdinalIgnoreCase))
            {
                VMess.VServer[] servers = VMess.ServerFromJson(json);
                if (servers != null)
                {
                    return new V2RayServer
                    {
                        HostAddress = servers[0].address,
                        HostPort = servers[0].port,
                        FriendlyName = $"{servers[0].address} - {servers[0].port}",

                        Protocol = "VMess",

                        Json = json,
                        jsonObject = servers,
                    };
                }
            }

            return null;
        }
        #endregion
    }
}
