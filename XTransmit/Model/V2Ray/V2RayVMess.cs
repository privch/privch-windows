using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using XTransmit.Utility;

namespace XTransmit.Model.V2Ray
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public class V2RayVMess : BaseServer
    {
        #region Public-Static
        public static List<string> VMessSecurity => V2Ray.Protocol.VMess.VUser.Security;
        public static List<string> StreamNetwork => V2Ray.Protocol.Transport.StreamSettings.Network;
        public static List<string> StreamSecurity => V2Ray.Protocol.Transport.StreamSettings.Security;
        #endregion

        #region Properties(Serializable)
        public string Protocol { get; } = "VMess";

        public string Identify
        {
            get => identify;
            set
            {
                identify = value;
                OnPropertyChanged(nameof(Identify));
            }
        }

        public int AlterId
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
        #endregion

        // values 
        private string identify;
        private int alterId;
        private string network;
        private string type;
        private string host;
        private string tls;
        private string path;

        public V2RayVMess()
        {
            identify = string.Empty;
            alterId = 0; // recommand value is 4, default value is 0
            network = string.Empty;
            type = string.Empty;
            host = string.Empty;
            tls = string.Empty;
            path = string.Empty;
        }

        public V2RayVMess Copy()
        {
            return (V2RayVMess)TextUtil.CopyBySerializer(this);
        }

        #region Import
        public static List<V2RayVMess> ImportServers(string vmessBase64List)
        {
            List<V2RayVMess> serverList = new List<V2RayVMess>();
            if (string.IsNullOrWhiteSpace(vmessBase64List))
            {
                return serverList;
            }

            // split items
            string[] vmessBase64Items = vmessBase64List.Split(
                new string[] { "\r\n", "\r", "\n", " " },
                StringSplitOptions.RemoveEmptyEntries);

            // deserialize items
            foreach (string vmessBase64 in vmessBase64Items)
            {
                if (FromVMessBase64(vmessBase64.Trim()) is V2RayVMess server)
                {
                    serverList.Add(server);
                }
            }

            return serverList;
        }

        // start with vmess://
        public static V2RayVMess FromVMessBase64(string vmessBase64)
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
                V2RayVMess server = new V2RayVMess
                {
                    HostAddress = vmessUri.address,
                    HostPort = vmessUri.port,

                    Identify = vmessUri.id,
                    AlterId = vmessUri.alterId,
                    Network = vmessUri.network,
                    Type = vmessUri.type,
                    Host = vmessUri.host,
                    Tls = vmessUri.tls,
                    Path = vmessUri.path,
                    Remarks = vmessUri.ps,
                };

                server.SetFriendlyNameDefault();
                return server;
            }

            return null;
        }
        #endregion

        public static string ToJson(V2RayVMess server)
        {
            if (server == null)
            {
                return null;
            }

            Protocol.Outbound outbound = new Protocol.Outbound
            {
                protocol = "vmess",

                settings = new Protocol.VMess
                {
                    vnext = new Protocol.VMess.VServer[]
                    {
                        new Protocol.VMess.VServer
                        {
                            address = server.HostAddress,
                            port = server.HostPort,
                            users = new Protocol.VMess.VUser[]
                            {
                                new Protocol.VMess.VUser
                                {
                                    id = server.identify,
                                    alterId = server.alterId,
                                }
                            }
                        }
                    }
                },

                streamSettings = new Protocol.Transport.StreamSettings
                {
                    network = server.network,
                    security = server.tls,
                    wsSettings = new Protocol.Transport.WebSocket
                    {
                        path = server.host,
                    }
                },

                mux = new Protocol.Mux
                {
                    enabled = true,
                },
            };

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            string json = JsonSerializer.Serialize(outbound, typeof(Protocol.Outbound), options);
            return json;
        }
    }
}
