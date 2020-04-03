using Privch.Model.V2Ray;
using Privch.ViewModel.Element;

namespace Privch.ViewModel
{
    class ServerConfigV2RayVModel : BaseViewModel
    {
        public V2RayVMess ServerEdit { get; }

        public ServerConfigV2RayVModel(V2RayVMess server)
        {
            ServerEdit = server;
        }
    }
}
