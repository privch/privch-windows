using Privch.Model.SS;
using Privch.ViewModel.Element;

namespace Privch.ViewModel
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
