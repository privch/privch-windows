using System.Diagnostics.CodeAnalysis;

namespace XTransmit.ViewModel.Element
{
    [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
    public class ItemView
    {
        public MaterialDesignThemes.Wpf.PackIconKind IconKind { get; set; }
        public string Label { get; set; } = null;
        public string Text { get; set; } = null;
        public string Uri { get; set; } = null;
    }
}
