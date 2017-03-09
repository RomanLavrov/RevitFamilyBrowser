using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFamilyBrowser.WPF_Classes
{
    public class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Xstart { get; set; }
        public int Ystart { get; set; }
        public int Xend { get; set; }
        public int Yend { get; set; }

        public double Length(Coordinates coord)
        {
            return Math.Sqrt(Math.Pow((coord.Xstart - coord.Xend), 2) + Math.Pow((coord.Ystart - coord.Yend), 2));
        }
    }
}
