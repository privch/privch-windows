using System.Windows;

namespace XTransmit.View
{
    public partial class DialogAction : Window
    {
        public bool? CancelableResult { get; set; } = null;

        public DialogAction(string title, string message)
        {
            InitializeComponent();
            DataContext = new ViewModel.DialogButtonVModel(title, message);
        }
    }
}
