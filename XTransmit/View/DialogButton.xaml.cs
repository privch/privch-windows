using System.Windows;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-06
     */
    public partial class DialogButton : Window
    {
        public bool? CancelableResult = null;

        public DialogButton(string title, string message)
        {
            InitializeComponent();
            DataContext = new ViewModel.DialogButtonVModel(title, message);
        }
    }
}
