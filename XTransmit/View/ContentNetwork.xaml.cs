using System.Windows.Controls;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class ContentNetwork : UserControl
    {
        public ContentNetwork()
        {
            InitializeComponent();
            DataContext = new ContentNetworkVModel();
        }

        // called once at startup
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ContentNetworkVModel viewModel)
            {
                viewModel.UpdateNetworkInterface();
            }
        }
    }
}
