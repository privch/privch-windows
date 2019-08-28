using System.Windows;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-06
     */
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
