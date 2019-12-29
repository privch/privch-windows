using System;

namespace XTransmit.ViewModel.Control
{
    /** 
     * A parameterless costructor is needed by serializer.
     */
    [Serializable]
    public class Placement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double W { get; set; }
        public double H { get; set; }
    }
}
