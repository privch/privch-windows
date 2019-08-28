using System.Windows.Controls;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /** Curl website list
     * Updated: 2019-08-02
     */
    public partial class ContentCurl : UserControl
    {
        public ContentCurl()
        {
            InitializeComponent();
            DataContext = new ContentCurlVModel();
        }
    }
}
