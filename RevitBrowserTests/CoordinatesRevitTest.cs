using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevitFamilyBrowser.Pattern_Elements_Install;
using System.Windows.Shapes;
using System.Drawing;

namespace RevitBrowserTests
{
    [TestClass]
    public class CoordinatesRevitTest
    {
        CoordinatesRevit coordinates = new CoordinatesRevit();
        
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

        [TestMethod]
        public void GetSlopeTest()
        {
            double actual = coordinates.GetSlope(testLineA);
            const double expected = 1;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetIntersectionTest()
        {
            PointF actual = coordinates.GetIntersection(testLineA, testLineB);
            PointF expected = new PointF
            {
                X = 5,
                Y = 5
            };
            Assert.AreEqual(expected, actual);
        }
    }
}
