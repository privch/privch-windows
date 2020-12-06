using PrivCh.Model.SS;
using PrivCh.ViewModel.Element;

namespace PrivCh.ViewModel
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
