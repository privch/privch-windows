namespace XTransmit.ViewModel.Control
{
    public class ProgressView
    {
        public int Value { get; set; } = 0;
        public bool IsIndeterminate { get; set; } = false;

        public ProgressView(int value, bool isIndeterminate)
        {
            Value = value;
            IsIndeterminate = isIndeterminate;
        }

        public void Set(int value, bool isIndeterminate)
        {
            Value = value;
            IsIndeterminate = isIndeterminate;
        }
    }
}
