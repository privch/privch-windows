using System.Diagnostics.CodeAnalysis;

namespace XTransmit.ViewModel.Control
{
    [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
    public class ItemView
    {
        public MaterialDesignThemes.Wpf.PackIconKind IconKind { get; set; }
        public string Label { get; set; }
        public string Text { get; set; }
        public string Uri { get; set; }
    }
}
