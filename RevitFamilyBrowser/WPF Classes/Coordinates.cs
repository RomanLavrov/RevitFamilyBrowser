using RevitFamilyBrowser.Revit_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RevitFamilyBrowser.WPF_Classes
{
    public class ProcessCoordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Xstart { get; set; }
        public int Ystart { get; set; }
        public int Xend { get; set; }
        public int Yend { get; set; }

        //-----Return Line length
        public double GetLength(Line line)
        {
            return Math.Sqrt(Math.Pow((line.X1 - line.X2), 2) + Math.Pow((line.Y1 - line.Y2), 2));
        }
        //-----Return Line slope
        public double GetSlope(Line line)
        {
            return (line.Y2 - line.Y1) / (line.X2 - line.X1);
        }
        public Point GetCenter(Line line)
        {
            Point point = new Point();
            point.X = (int)(line.X2 + line.X1) / 2;
            point.Y = (int)(line.Y2 + line.Y1) / 2;
            return point;
        }

        //-----Return coefficients of Line equation ax+by+c=0
        public static List<double> LineEquation(Line line)
        {
            List<double> result = new List<double>();
            double a = line.Y2 - line.Y1;
            double b = line.X1 - line.X2;
            double c = line.Y1 * (line.X1 - line.X2) - line.X1 * (line.Y1 - line.Y2);
            result.Add(a);
            result.Add(b);
            result.Add(-c);
            return result;
        }

        //-----Get two lines and return intersection point
        public Point GetIntersection(Line box, Line wall)
        {
            List<double> wallCoefs = LineEquation(wall);
            double a1 = wallCoefs[0];
            double b1 = wallCoefs[1];
            double c1 = wallCoefs[2];

            List<double> boxCoefs = LineEquation(box);
            double a2 = boxCoefs[0];
            double b2 = boxCoefs[1];
            double c2 = boxCoefs[2];

            double x = (int)(c1 * b2 - c2 * b1) / (a2 * b1 - a1 * b2);
            double y = 0;
            if (b1 == 0)
            {
                y = (int)(-c2 - a2 * x) / b2;
            }
            else if (b2 == 0)
                y = (int)(-c1 - a1 * x) / b1;

            else
                y = (-c2 - (a2 * x)) / b2;

            Point intersection = new Point();
            intersection.X = (int)x;
            intersection.Y = (int)y;

            return intersection;
        }

        //-----Check if point belongs to line
        public bool IntersectionPositionCheck(Line line, Point point)
        {
            int lineMaxX = (int)(line.X1 > line.X2 ? line.X1 : line.X2);
            int lineMinX = (int)(line.X1 < line.X2 ? line.X1 : line.X2);
            int lineMaxY = (int)(line.Y1 > line.Y2 ? line.Y1 : line.Y2);
            int lineMinY = (int)(line.Y1 < line.Y2 ? line.Y1 : line.Y2);

            if (point.X <= lineMaxX && point.X >= lineMinX && point.Y <= lineMaxY && point.Y >= lineMinY)
            {
                return true;
            }
            return false;
        }

        //-----If line is paralle to one of axis replace infinity coord to defined
        public Line OrtoNormalization(Line perpend)
        {
            Line normal = new Line();
            int xA = 0; int xB = 0;
            int yA = 0; int yB = 0;

            if (perpend.X1 != int.MaxValue || perpend.X1 != int.MinValue)
            {
                xA = (int)perpend.X1;
            }
            if (perpend.Y1 != int.MaxValue || perpend.Y1 != int.MinValue)
            {
                yA = (int)perpend.Y1;
            }
            if (perpend.X2 != int.MaxValue || perpend.X2 != int.MinValue)
            {
                xB = (int)perpend.X2;
            }
            if (perpend.Y2 != int.MaxValue || perpend.Y2 != int.MinValue)
            {
                yB = (int)perpend.Y2;
            }

            if ((xA == int.MaxValue || xA == int.MinValue) && yA == 0)
            {
                yA = 0; xA = xB;
            }

            if ((xB == int.MaxValue || xB == int.MinValue) && yB == 0)
            {
                xB = 0; yB = yA;
            }

            if ((yA == int.MaxValue || yA == int.MinValue) && xA == 0)
            {
                yA = 0; xA = xB;
            }

            if ((yB == int.MaxValue || yB == int.MinValue) && xB == 0)
            {
                yB = 0; xB = xA;
            }
            normal.X1 = xA;
            normal.X2 = xB;
            normal.Y1 = yA;
            normal.Y2 = yB;

            return normal;
        }

        public Line BuildBoundedLine(List<Line> boundingBox, Line perpend)
        {
            Line gridLine = new Line();
            Line normal = OrtoNormalization(perpend);         
     
            List<Point> allIntersections = new List<Point>();
            foreach (var side in boundingBox)
            {                
                Point intersection = GetIntersection(side, normal);
                if (IntersectionPositionCheck(side, intersection))
                {
                    allIntersections.Add(intersection);
                }
            }
                        
            int count = 0;
            foreach (var item in allIntersections)
            {
                count++;
                if (count == 1)
                {
                    gridLine.X1 = item.X;
                    gridLine.Y1 = item.Y;
                }
                else if (count == 2)
                {
                    gridLine.X2 = item.X;
                    gridLine.Y2 = item.Y;
                }               
            }
            return DrawDashedLine(gridLine);
        }

        public List<Point> SplitLineProportional(Line line, int lineNumber)
        {
            List<Point> points = new List<Point>();
            int partNumber = lineNumber * 2;
            for (int i = 1; i < partNumber; i=i+2)
            {
                Point point = new Point();
                double top = i;
                double bottom = (partNumber - i);
                double proportion;
                if ((partNumber - i) == 0)
                {
                    bottom = 1;
                }
                proportion = top / bottom;
                point.X = Convert.ToInt32((line.X1 + (line.X2 * proportion)) / (1 + proportion));
                point.Y = Convert.ToInt32((line.Y1 + (line.Y2 * proportion)) / (1 + proportion));
                points.Add(point);
            }
            return points;
        }

        public List<Point> SplitLine(Line line, int lineNumber)
        {
            List<Point> points = new List<Point>();

            int partNumber = lineNumber + 1;
            for (int i = 1; i < partNumber; i++)
            {
                Point point = new Point();
                double top = i;
                double bottom = (partNumber - i);
                double proportion;
                if ((partNumber - i) == 0)
                {
                    bottom = 1;
                }
                proportion = top / bottom;
                point.X = Convert.ToInt32((line.X1 + (line.X2 * proportion)) / (1 + proportion));
                point.Y = Convert.ToInt32((line.Y1 + (line.Y2 * proportion)) / (1 + proportion));
                points.Add(point);
            }
            return points;
        }

        public List<Line> GetBoundingBox(ConversionPoint min, ConversionPoint max, int Scale, int derX, int derY)
        {
            List<Line> boxSides = new List<Line>();
            int offset = 500 / Scale;

            Line SideA = new Line();
            SideA.X1 = min.X / Scale + derX - offset;
            SideA.Y1 = -min.Y / Scale + derY + offset;
            SideA.X2 = min.X / Scale + derX - offset;
            SideA.Y2 = -max.Y / Scale + derY - offset;
            boxSides.Add(SideA);

            Line SideB = new Line();
            SideB.X1 = min.X / Scale + derX - offset;
            SideB.Y1 = -max.Y / Scale + derY - offset;
            SideB.X2 = max.X / Scale + derX + offset;
            SideB.Y2 = -max.Y / Scale + derY - offset;
            boxSides.Add(SideB);

            Line SideC = new Line();
            SideC.X1 = max.X / Scale + derX + offset;
            SideC.Y1 = -max.Y / Scale + derY - offset;
            SideC.X2 = max.X / Scale + derX + offset;
            SideC.Y2 = -min.Y / Scale + derY + offset;
            boxSides.Add(SideC);

            Line SideD = new Line();
            SideD.X1 = max.X / Scale + derX + offset;
            SideD.Y1 = -min.Y / Scale + derY + offset;
            SideD.X2 = min.X / Scale + derX - offset;
            SideD.Y2 = -min.Y / Scale + derY + offset;
            boxSides.Add(SideD);
            foreach (var item in boxSides)
            {
                item.Stroke = System.Windows.Media.Brushes.Transparent;
            }
            return boxSides;
        }

        public List<Line> DrawPerp(Line baseWall, List<Point> points)
        {
            List<Line> lines = new List<Line>();
            double slope = -1 / (GetSlope(baseWall));

            foreach (Point point in points)
            {
                Point target = new Point();
                target.X = 0;
                target.Y = (int)(point.Y - (slope * point.X));

                Line perpendicular = new Line();
                perpendicular.X1 = target.X;
                perpendicular.Y1 = target.Y;
                perpendicular.X2 = point.X;
                perpendicular.Y2 = point.Y;
                perpendicular.Stroke = System.Windows.Media.Brushes.Red;
                lines.Add(perpendicular);
            }
            return lines;
        }

        //-----Draw Center Line on Canvas by given Start and End points
        public System.Windows.Shapes.Line DrawCenterLine(System.Windows.Shapes.Line line)
        {
            line.Stroke = System.Windows.Media.Brushes.Red;
            DoubleCollection dash = new DoubleCollection() { 130, 8, 15, 8 };
            line.StrokeDashArray = dash;            
            return line;
        }

        //-----Draw Dashed Line on Canvas by given Start and End points
        public Line DrawDashedLine(System.Windows.Shapes.Line line)
        {
            line.Stroke = System.Windows.Media.Brushes.SteelBlue;
            DoubleCollection dash = new DoubleCollection() { 20, 10, 20, 10 };
            line.StrokeDashArray = dash;           
            return line;
        }
    }
}
