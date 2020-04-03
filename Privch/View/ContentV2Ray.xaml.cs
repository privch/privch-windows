using System.Windows.Controls;
using Privch.ViewModel;

namespace Privch.View
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
