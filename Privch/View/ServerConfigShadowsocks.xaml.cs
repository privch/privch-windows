using System.Windows.Controls;
using Privch.Model.SS;
using Privch.ViewModel;

namespace Privch.View
{
    public partial class ServerConfigShadowsocks : UserControl
    {
        public ServerConfigShadowsocks(Shadowsocks server)
        {
            InitializeComponent();
            DataContext = new ServerConfigShadowsocksVModel(server);
        }
    }
}
