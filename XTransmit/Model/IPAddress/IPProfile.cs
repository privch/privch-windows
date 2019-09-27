using System;
using System.ComponentModel;

namespace XTransmit.Model.IPAddress
{
    /**
     * Updated: 2019-09-28
     */
    [Serializable]
    public class IPProfile : INotifyPropertyChanged
    {
        public string IP { get; set; }
        public string Remarks { get; set; }
        public long Ping
        {
            get { return v_ping; }
            set
            {
                v_ping = value;
                OnPropertyChanged("Ping");
            }
        }

        public IPProfile()
        {
            IP = "100.100.100.100";
            Remarks = "";
            v_ping = 0;
        }

        //TODO - Create a ViewModel for IPProfile?
        private long v_ping;

        /** INotifyPropertyChanged, Crap
         */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
