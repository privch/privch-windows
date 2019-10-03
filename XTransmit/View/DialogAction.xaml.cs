using System.Windows;

namespace XTransmit.View
{
    /**
     * Updated: 2019-10-02
     */
    public partial class DialogAction : Window
    {
        public bool? CancelableResult = null;

        public DialogAction(string title, string message)
        {
            InitializeComponent();
            DataContext = new ViewModel.DialogButtonVModel(title, message);
        }
    }
}
