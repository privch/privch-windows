using System.Windows.Controls;
using XTransmit.Model.V2Ray;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class ServerConfigV2Ray : UserControl
    {
        public ServerConfigV2Ray(V2RayVMess server)
        {
            InitializeComponent();
            DataContext = new ServerConfigV2RayVModel(server);
        }
    }
}
