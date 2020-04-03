using System.Windows.Controls;
using Privch.Model.V2Ray;
using Privch.ViewModel;

namespace Privch.View
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
