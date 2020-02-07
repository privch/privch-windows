using System.Windows.Controls;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class ContentShadowsocks : UserControl
    {
        public ContentShadowsocks()
        {
            InitializeComponent();
            DataContext = new ContentShadowsocksVModel();
        }
    }
}
