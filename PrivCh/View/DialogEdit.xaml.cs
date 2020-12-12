using System;
using System.Windows;

namespace PrivCh.View
{
    // TODO - Not used yet
    public partial class DialogEdit : Window
    {
        public DialogEdit(string title, string message, Action action)
        {
            InitializeComponent();
            DataContext = new ViewModel.DialogEditVModel(title, message, action);
        }
    }
}
