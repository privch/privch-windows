using XTransmit.Model.V2Ray;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
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
