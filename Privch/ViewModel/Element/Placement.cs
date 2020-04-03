using System;
using System.Windows;

namespace Privch.ViewModel.Element
{
    /** 
     * A parameterless costructor is needed by serializer.
     */
    [Serializable]
    public class Placement
    {
        public double X
        {
            get => vx;
            set
            {
                vx = value;
                if (vx < 0) vx = 0;
                if (vx >= SystemParameters.PrimaryScreenWidth)
                {
                    vx = SystemParameters.PrimaryScreenWidth - 10;
                }
            }
        }

        public double Y
        {
            get => vy;
            set
            {
                vy = value;
                if (vy < 0) vy = 0;
                if (vy >= SystemParameters.PrimaryScreenHeight)
                {
                    vy = SystemParameters.PrimaryScreenHeight - 10;
                }
            }
        }

        public double W
        {
            get => vw;
            set
            {
                vw = value;
                if (vw < 1 || vw >= SystemParameters.PrimaryScreenWidth)
                {
                    vw = SystemParameters.PrimaryScreenWidth / 2;
                }
            }
        }

        public double H
        {
            get => vh;
            set
            {
                vh = value;
                if (vh < 1 || vh >= SystemParameters.PrimaryScreenHeight)
                {
                    vh = SystemParameters.PrimaryScreenHeight / 2;
                }
            }
        }

        private double vx = 0;
        private double vy = 0;
        private double vw = 0;
        private double vh = 0;
    }
}
