using System;
using System.Collections.Generic;
using System.Windows;

namespace Privch.View
{
    public partial class DialogAction : Window
    {
        public DialogAction(string title, string message, Dictionary<string, Action> actions)
        {
            InitializeComponent();
            DataContext = new ViewModel.DialogActionVModel(this, title, message, actions);
        }
    }
}
