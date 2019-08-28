using System.Windows.Controls;

namespace XTransmit.ViewModel.Control
{
    /**
     * Updated: 2019-08-02
     */
    public class ContentTable
    {
        public string Title { get; set; }
        public UserControl Content { get; set; }

        // Store IsChecked property of a RadioButton, for saving the display status.
        public bool IsChecked { get; set; } 

        public ContentTable(string title, UserControl content)
        {
            IsChecked = false;
            Title = title;
            Content = content;
        }
    }
}
