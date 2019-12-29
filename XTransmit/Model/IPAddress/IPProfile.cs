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
            get { return ping_delay; }
            set
            {
                ping_delay = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        public IPProfile()
        {
            IP = "100.100.100.100";
            Remarks = "";
            ping_delay = 0;
        }

        //TODO - Create a ViewModel for IPProfile
        private long ping_delay;

        /** INotifyPropertyChanged, Crap
         */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
