using System.Windows.Controls;

using PrivCh.ViewModel;

namespace PrivCh.View
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
