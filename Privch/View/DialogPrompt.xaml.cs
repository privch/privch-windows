using System.Windows;

namespace Privch.View
{
    public partial class DialogPrompt : Window
    {
        public DialogPrompt(string title, string message)
        {
            InitializeComponent();

            Title = title;
            xTextBox.Text = message;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
