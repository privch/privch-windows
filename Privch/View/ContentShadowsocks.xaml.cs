using System.Windows.Controls;
using Privch.ViewModel;

namespace Privch.View
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
