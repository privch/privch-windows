using System.Windows.Controls;
using XTransmit.Model.SS;
using XTransmit.ViewModel;

namespace XTransmit.View
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
