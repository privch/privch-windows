using System.Windows;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-06
     */
    public partial class DialogEdit : Window
    {
        public string EditText;

        public DialogEdit(string title, string message)
        {
            InitializeComponent();
            DataContext = new ViewModel.DialogEditVModel(title, message);
        }
    }
}
