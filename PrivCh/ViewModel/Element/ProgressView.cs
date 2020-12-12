namespace PrivCh.ViewModel.Element
{
    public class ProgressView
    {
        public int Value { get; set; }
        public bool IsIndeterminate { get; set; }
        public string Description { get; set; }

        public ProgressView(int value, bool isIndeterminate, string description)
        {
            Value = value;
            IsIndeterminate = isIndeterminate;
            Description = description;
        }

        public void Set(int value, bool isIndeterminate, string description)
        {
            Value = value;
            IsIndeterminate = isIndeterminate;
            Description = description;
        }
    }
}
