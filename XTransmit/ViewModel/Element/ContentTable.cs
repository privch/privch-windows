using System.Windows.Controls;

namespace XTransmit.ViewModel.Element
{
    public class ContentTable
    {
        public string Title { get; }
        public UserControl Content { get; }

        // Store the IsChecked property value of a RadioButton for saving the table display status.
        public bool IsChecked { get; set; }

        public ContentTable(string title, UserControl content)
        {
            IsChecked = false;
            Title = title;
            Content = content;
        }
    }
}
