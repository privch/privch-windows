using System.Windows;

namespace XTransmit.View
{
    public partial class DialogEdit : Window
    {
        public string EditText { get; set; }

        public DialogEdit(string title, string message)
        {
            InitializeComponent();
            DataContext = new ViewModel.DialogEditVModel(title, message);
        }
    }
}
