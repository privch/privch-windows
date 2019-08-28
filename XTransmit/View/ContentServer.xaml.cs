using System.Windows.Controls;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-06
     */
    public partial class ContentServer : UserControl
    {
        public ContentServer()
        {
            InitializeComponent();
            DataContext = new ContentServerVModel();
        }
    }
}
