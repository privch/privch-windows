using System.Windows.Controls;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /// <summary>
    /// Interaction logic for ContentV2Ray.xaml
    /// </summary>
    public partial class ContentV2Ray : UserControl
    {
        public ContentV2Ray()
        {
            InitializeComponent();
            DataContext = new ContentV2RayVModel();
        }
    }
}
