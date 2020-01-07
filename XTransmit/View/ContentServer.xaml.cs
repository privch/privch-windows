using System.Windows.Controls;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class ContentServer : UserControl
    {
        public ContentServer()
        {
            InitializeComponent();
            DataContext = new ContentServerVModel();
        }
    }
}
