using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFamilyBrowser.Revit_Classes
{
    //Convert coordinates from feet to milimeters

    public class ConversionPoint : IComparable<ConversionPoint>
    {
        public int X { get; set; }
        public int Y { get; set; }

        const double conversion = 25.4 * 12;

        static int ConvertFeetToMils (double d)
        {
            return Convert.ToInt32(conversion * d +0.5);
        }

        public ConversionPoint (XYZ p)
        {
            X = ConvertFeetToMils(p.X);
            Y = ConvertFeetToMils(p.Y);
        }

        public int CompareTo(ConversionPoint point)
        {
            int difference = X - point.X;

            if (difference == 0)
            {
                difference = Y - point.Y;
            }
            return difference;
        }
        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }
    }
}
