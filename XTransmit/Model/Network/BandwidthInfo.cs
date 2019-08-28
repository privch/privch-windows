using System;

namespace XTransmit.Model.Network
{
    /**
     * Updated: 2019-08-02
     */
    public class BandwidthInfo
    {
        public DateTime Time { get; set; }
        public long Value { get; set; }

        public BandwidthInfo(DateTime time, long value)
        {
            Time = time;
            Value = value;
        }
    }
}
