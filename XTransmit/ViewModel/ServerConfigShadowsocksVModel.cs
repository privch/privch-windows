using XTransmit.Model.SS;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    class ServerConfigShadowsocksVModel : BaseViewModel
    {
        public Shadowsocks ServerEdit { get; }

        public ServerConfigShadowsocksVModel(Shadowsocks server)
        {
            ServerEdit = server;
        }
    }
}
