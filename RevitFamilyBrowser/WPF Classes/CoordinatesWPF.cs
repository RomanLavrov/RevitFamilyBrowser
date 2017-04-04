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
    public class WpfCoordinates
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Xstart { get; set; }
        public double Ystart { get; set; }
        public double Xend { get; set; }
        public double Yend { get; set; }

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
        //public Autodesk.Revit.DB.Point GetRevitIntersection(Autodesk.Revit.DB.Line box, Autodesk.Revit.DB.Line wall )
        //-----Get two lines and return intersection point
        public Point GetIntersection(Line box, Line wall)
        {
            Line normalBox = OrtoNormalization(box);
            Line normalWall = OrtoNormalization(wall);

            List<double> wallCoefs = LineEquation(normalWall);
            double a1 = wallCoefs[0];
            double b1 = wallCoefs[1];
            double c1 = wallCoefs[2];

            List<double> boxCoefs = LineEquation(normalBox);
            double a2 = boxCoefs[0];
            double b2 = boxCoefs[1];
            double c2 = boxCoefs[2];
            Point intersection = new Point();

            {
                double x = (c1 * b2 - c2 * b1) / (a2 * b1 - a1 * b2);
                double y = 0;
                if (b1 == 0)
                {
                    y = (int)(-c2 - a2 * x) / b2;
                }
                else if (b2 == 0)
                    y = (-c1 - a1 * x) / b1;

                else
                    y = (-c2 - (a2 * x)) / b2;

                intersection.X = (int)x;
                intersection.Y = (int)y;
            }
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

        //-----Build dashed line in limits of box around the room
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
            for (int i = 1; i < partNumber; i = i + 2)
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

        //public List<Point> revitInstallPoints (List<Point> wpfPoints, int Scale, int derX, int derY)
        //{
        //    List<Point> revitPoints = new List<Point>();
        //    foreach (var wpfPoint in wpfPoints)
        //    {
        //        Point point = new Point();
        //        point.X = wpfPoint.X * Scale - derX;
        //        point.Y = -wpfPoint.Y * Scale - derY;
        //    }
        //    return revitPoints;
        //}
        //-----Create 4 lines in given scale and central position around selected room
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

        //-----Create perpendiculars to given line in given points
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

        public List<Point> GetGridPoints(List<Line> listPerpendiculars, List<List<Line>> wallNormals)
        {
            List<Point> gridPoints = new List<Point>();
            if (listPerpendiculars.Count > 0)
            {
                wallNormals.Add(listPerpendiculars);
                // System.Windows.MessageBox.Show("Walls with perpendiculars = " + wallNormals.Count.ToString());
            }
            List<System.Drawing.Point> temp = new List<System.Drawing.Point>();

            foreach (var normalA in wallNormals)
            {
                foreach (var lineA in normalA)
                {
                    foreach (var normalB in wallNormals)
                    {
                        foreach (var lineB in normalB)
                        {
                            if (lineA != lineB)
                                temp.Add(GetIntersection(lineA, lineB));
                        }
                    }
                }
            }

            IEnumerable<System.Drawing.Point> distinctPoints = temp.Distinct();
            List<Point> filteredPoints = new List<Point>();
            foreach (var item in distinctPoints)
            {
                if (item.Y != int.MaxValue && item.Y != int.MinValue)
                {
                    filteredPoints.Add(item);
                }
            }
            return filteredPoints;
        }

        public List<Point> GetIntersectInRoom(List<Line> boundingBox, List<Point> gridPoints)
        {
            List<Point> internalPoints = new List<Point>();
            double roomMinX = boundingBox[0].X1;
            double roomMinY = boundingBox[0].Y1;
            double roomMaxX = boundingBox[0].X1;
            double roomMaxY = boundingBox[0].Y1;
            string temp = string.Empty;

            foreach (var item in boundingBox)
            {
                if (item.X1 < roomMinX || item.X2 < roomMinX)
                    roomMinX = item.X1 < item.X2 ? item.X1 : item.X2;

                if (item.Y1 < roomMinY || item.Y2 < roomMinY)
                    roomMinY = item.Y1 < item.Y2 ? item.Y1 : item.Y2;

                if (item.X1 > roomMinX || item.X2 > roomMinX)
                    roomMaxX = item.X1 > item.X2 ? item.X1 : item.X2;

                if (item.Y1 > roomMaxY || item.Y2 > roomMaxY)
                    roomMaxY = item.Y1 > item.Y2 ? item.Y1 : item.Y2;
            }

            foreach (var item in gridPoints)
            {
                if (item.X > roomMinX && item.X < roomMaxX &&
                    item.Y > roomMinY && item.Y < roomMaxY)
                {
                    internalPoints.Add(item);
                }
            }
            return internalPoints;
        }
    }
}
