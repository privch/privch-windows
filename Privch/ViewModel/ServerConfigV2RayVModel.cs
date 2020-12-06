using PrivCh.Model.V2Ray;
using PrivCh.ViewModel.Element;

namespace PrivCh.ViewModel
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
