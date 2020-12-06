using System.Windows.Controls;
using PrivCh.Model.SS;
using PrivCh.ViewModel;

namespace PrivCh.View
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
