using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zRevitFamilyBrowser.Pattern_Elements_Install;
using System.Windows.Shapes;
using System.Drawing;

namespace RevitBrowserTests
{
    [TestClass]
    public class CoordinatesRevitTest
    {
      
        Line testLineA = new Line
        {
            X1 = 0,
            Y1 = 0,
            X2 = 10,
            Y2 = 10
        };

        Line testLineB = new Line
        {
            X1 = 0,
            Y1 = 10,
            X2 = 10,
            Y2 = 0
        };

       
    }
}
