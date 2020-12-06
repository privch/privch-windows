using System.Windows.Controls;
using PrivCh.Model.V2Ray;
using PrivCh.ViewModel;

namespace PrivCh.View
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
