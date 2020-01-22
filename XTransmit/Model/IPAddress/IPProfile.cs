using System;
using System.ComponentModel;

namespace XTransmit.Model.IPAddress
{
    [Serializable]
    public class IPProfile : INotifyPropertyChanged
    {
        public string IP { get; set; }
        public string Remarks { get; set; }
        public long Ping
        {
            get { return pingValue; }
            set
            {
                pingValue = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        public IPProfile()
        {
            IP = "100.100.100.100";
            Remarks = "";
            pingValue = 0;
        }

        // related function may be removed
        private long pingValue;


        /** INotifyPropertyChanged, Crap
         */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
