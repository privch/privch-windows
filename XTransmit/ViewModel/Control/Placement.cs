using System;

namespace XTransmit.ViewModel.Control
{
    /** A parameterless costructor is needed by serializer.
     * 
     * Updated: 2019-08-02
     */
    [Serializable]
    public class Placement
    {
        public double X, Y, W, H;
    }
}
