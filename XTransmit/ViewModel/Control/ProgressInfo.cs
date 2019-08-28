namespace XTransmit.ViewModel.Control
{
    /**
     * Updated: 2019-08-02
     */
    public class ProgressInfo
    {
        public int Value { get; set; }
        public bool IsIndeterminate { get; set; }

        public ProgressInfo(int value, bool isIndeterminate)
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
