using RevitFamilyBrowser.Revit_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Drawing.Point;

namespace RevitFamilyBrowser.WPF_Classes
{
    public class WpfCoordinates
    {
        public Line BuildInstallAxis(List<Line> boundingBox, Line perpend)
        {
            Line gridLine = new Line();
            PointF max = BoundingBoxMax(boundingBox);
            PointF min = BoundingBoxMin(boundingBox);

            List<PointF> allIntersections = new List<PointF>();
            string temp = string.Empty;
            foreach (var side in boundingBox)
            {
                PointF intersection = GetIntersection(side, perpend);
                if (!float.IsInfinity(intersection.X) && !float.IsInfinity(intersection.Y))
                {
                    if (intersection.X <= max.X && intersection.X >= min.X
                        && intersection.Y <= (max.Y + 0.001) && intersection.Y >= (min.Y - 0.001))
                    {
                        temp += ("X = " + intersection.X + "; Y = " + intersection.Y + "\n");
                        allIntersections.Add(intersection);
                    }
                }
            }
           // MessageBox.Show(temp);
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

        public List<PointF> GetGridPoints(List<Line> listPerpendiculars, List<Line> wallNormals)
        {
            List<PointF> temp = new List<PointF>();
            wallNormals.AddRange(listPerpendiculars);

            foreach (var normalA in wallNormals)
            {
                foreach (var normalB in wallNormals)
                {
                    if (!normalA.Equals(normalB))
                        temp.Add(GetIntersection(normalA, normalB));
                }
            }

            IEnumerable<PointF> distinctPoints = temp.Distinct();
            List<PointF> filteredPoints = new List<PointF>();
            foreach (var item in distinctPoints)
            {
                if (!float.IsInfinity(item.X) && !float.IsNaN(item.X) && !float.IsInfinity(item.Y) && !float.IsNaN(item.Y))
                {
                    filteredPoints.Add(item);
                }
            }
            return filteredPoints;
        }

        public PointF GetIntersection(Line box, Line wall)
        {
            List<double> wallCoefs = LineEquation(wall);
            double a1 = wallCoefs[0];
            double b1 = wallCoefs[1];
            double c1 = wallCoefs[2];

            List<double> boxCoefs = LineEquation(box);
            double a2 = boxCoefs[0];
            double b2 = boxCoefs[1];
            double c2 = boxCoefs[2];

            PointF intersection = new PointF();
            {
                double x = (c1 * b2 - c2 * b1) / (a2 * b1 - a1 * b2);
                double y = 0;
                double tolerance = 0.0001;
                if (Math.Abs(b1) < tolerance)
                {
                    y = (-c2 - a2 * x) / b2;
                }
                else if (Math.Abs(b2) < tolerance)
                    y = (-c1 - a1 * x) / b1;

                else
                    y = (-c2 - (a2 * x)) / b2;

                if (x.Equals(Double.NaN))
                {
                    x = float.PositiveInfinity;
                }
                if (y.Equals(Double.NaN))
                {
                    y = float.PositiveInfinity;
                }
                intersection.X = (float)x;
                intersection.Y = (float)y;
            }
            return intersection;
        }

        private PointF BoundingBoxMin(List<Line> boundingBox)
        {
            PointF min = new PointF();
            min.X = (float)boundingBox[0].X1;
            min.Y = (float)boundingBox[0].Y1;

            foreach (Line side in boundingBox)
            {
                if (side.X1 < side.X2)
                {
                    if (min.X > side.X1)
                        min.X = (float)side.X1;
                }
                else
                {
                    if (min.X > side.X2)
                        min.X = (float)side.X2;
                }

                if (side.Y1 < side.Y2)
                {
                    if (min.Y > side.Y1)
                        min.Y = (float)side.Y1;
                }
                else
                {
                    if (min.Y > side.Y2)
                        min.Y = (float)side.Y2;
                }
            }
            return min;
        }
        private PointF BoundingBoxMax(List<Line> boundingBox)
        {
            PointF max = new PointF();
            max.X = (float)boundingBox[0].X1; ;
            max.Y = (float)boundingBox[0].Y1;

            foreach (Line side in boundingBox)
            {
                if (side.X1 < side.X2 )
                {
                    if (max.X < side.X2)
                        max.X = (float)side.X2;
                }
                else
                {
                    if (max.X < side.X1)
                        max.X = (float)side.X1;
                }

                if (side.Y1 < side.Y2)
                {
                    if (max.Y < side.Y2)
                        max.Y = (float)side.Y2;
                }
                else
                {
                    if (max.Y < side.Y1)
                        max.Y = (float)side.Y1;
                }
            }
            return max;
        }

        public List<PointF> SplitLineProportional(Line line, int lineNumber)
        {
            List<PointF> points = new List<PointF>();
            int partNumber = lineNumber * 2;
            for (int i = 1; i < partNumber; i = i + 2)
            {
                PointF point = new PointF();
                double top = i;
                double bottom = (partNumber - i);
                if ((partNumber - i) == 0)
                {
                    bottom = 1;
                }
                var proportion = top / bottom;
                point.X = (float)((line.X1 + (line.X2 * proportion)) / (1 + proportion));
                point.Y = (float)((line.Y1 + (line.Y2 * proportion)) / (1 + proportion));
                points.Add(point);
            }
            return points;
        }

        public List<PointF> SplitLineEqual(Line line, int lineNumber)
        {
            List<PointF> points = new List<PointF>();

            int partNumber = lineNumber + 1;
            for (int i = 1; i < partNumber; i++)
            {
                PointF point = new PointF();
                double top = i;
                double bottom = (partNumber - i);
                if ((partNumber - i) == 0)
                {
                    bottom = 1;
                }
                var proportion = top / bottom;
                point.X = (float)((line.X1 + (line.X2 * proportion)) / (1 + proportion));
                point.Y = (float)((line.Y1 + (line.Y2 * proportion)) / (1 + proportion));
                points.Add(point);
            }
            return points;
        }

        #region Split line on parts by segment lenght

        public List<PointF> SplitLineDistance(Line line, double distance)
        {
            List<PointF> points = new List<PointF>();
            List<double> partSizes = GetPartsSizes(line, distance);

            double tolerance = 0.01;
            if ((line.X1 > line.X2) && (Math.Abs(line.Y1 - line.Y2) < tolerance))
            {
               // MessageBox.Show("Ok X");
                Line reverse = new Line();
                reverse.X1 = line.X2;
                reverse.Y1 = line.Y2;
                reverse.X2 = line.X1;
                reverse.Y2 = line.Y1;
                foreach (var part in partSizes)
                {
                    points.Add(GetSecondCoord(reverse, part));
                }
            }

            else if ((Math.Abs(line.X1 - line.X2) < tolerance) && (line.Y1 > line.Y2))
            {
               // MessageBox.Show("Ok Y");
                Line reverse = new Line();
                reverse.X1 = line.X2;
                reverse.Y1 = line.Y2;
                reverse.X2 = line.X1;
                reverse.Y2 = line.Y1;
                foreach (var part in partSizes)
                {
                    points.Add(GetSecondCoord(reverse, part));
                }
            }

            else
            {
                foreach (var part in partSizes)
                {
                    points.Add(GetSecondCoord(line, part));
                }
            }


            return points;
        }

        private List<double> GetPartsSizes(Line line, double distance)
        {
            List<double> partLenghts = new List<double>();
            int parts = GetLinePartsNumber(line, distance);
            double firstPart = (GetLength(line) - ((parts - 1) * distance)) / 2;
            partLenghts.Add(firstPart);
            for (int i = 1; i <= parts - 1; i++)
            {
                double part = firstPart + i * distance;
                partLenghts.Add(part);
            }
            return partLenghts;
        }

        private int GetLinePartsNumber(Line line, double distance)
        {
            int parts;
            double wallLength = this.GetLength(line);
            if ((wallLength % distance).Equals(0) && wallLength / distance > 2)
            {
                parts = (int)(wallLength / distance) - 1;
            }
            else if (wallLength / distance <= 2 && wallLength > distance)
            {
                parts = 2;
            }
            else
            {
                parts = (int)Math.Truncate(wallLength / distance);
            }
            return parts;
        }
       
        public PointF GetSecondCoord(Line line, double distance)
        {
            PointF point = new PointF();
           
            double angle = GetAngle(line);
            point.X = (float)(line.X1 + distance * Math.Sin(angle));
            point.Y = (float)(line.Y1 + distance * Math.Cos(angle));

            return point;
        }

        public double GetAngle(Line line)
        {
            List<double> lineCoefs = LineEquation(line);
            double angle = -Math.Atan(lineCoefs[1] / lineCoefs[0]);
            return angle;
        }

        #endregion

        public List<Line> GetBoundingBox(PointF min, PointF max, GridSetup grid)
        {
            int scale = grid.Scale;
            float derX = grid.Derrivation.X;
            float derY = grid.Derrivation.Y;

            List<Line> boxSides = new List<Line>();
            int offset = 800 / scale;

            Line SideA = new Line();
            SideA.X1 = min.X / scale + derX - offset;
            SideA.Y1 = -min.Y / scale + derY + offset;
            SideA.X2 = min.X / scale + derX - offset;
            SideA.Y2 = -max.Y / scale + derY - offset;
            boxSides.Add(SideA);

            Line SideB = new Line();
            SideB.X1 = min.X / scale + derX - offset;
            SideB.Y1 = -max.Y / scale + derY - offset;
            SideB.X2 = max.X / scale + derX + offset;
            SideB.Y2 = -max.Y / scale + derY - offset;
            boxSides.Add(SideB);

            Line SideC = new Line();
            SideC.X1 = max.X / scale + derX + offset;
            SideC.Y1 = -max.Y / scale + derY - offset;
            SideC.X2 = max.X / scale + derX + offset;
            SideC.Y2 = -min.Y / scale + derY + offset;
            boxSides.Add(SideC);

            Line SideD = new Line();
            SideD.X1 = max.X / scale + derX + offset;
            SideD.Y1 = -min.Y / scale + derY + offset;
            SideD.X2 = min.X / scale + derX - offset;
            SideD.Y2 = -min.Y / scale + derY + offset;
            boxSides.Add(SideD);
            //foreach (var item in boxSides)
            //{
            //    item.Stroke = System.Windows.Media.Brushes.Aquamarine;
            //    grid.canvas.Children.Add(item);
            //}
            return boxSides;
        }

        public List<Line> GetPerpendiculars(Line baseWall, List<PointF> points)
        {
            List<Line> perpList = new List<Line>();

            foreach (PointF point in points)
            {
                PointF target = new PointF();
                double tolerance = 0.0000001;
                if (Math.Abs(GetSlope(baseWall)) < tolerance)
                {
                    target.X = point.X;
                    target.Y = 0;
                }
                else if (double.IsInfinity(GetSlope(baseWall)))
                {
                    target.X = 0;
                    target.Y = point.Y;
                }
                else
                {
                    double slope = -1 / GetSlope(baseWall);
                    target.X = 0;
                    target.Y = (float)(point.Y - (slope * point.X));
                }

                Line perpendicular = new Line();
                perpendicular.X1 = target.X;
                perpendicular.Y1 = target.Y;
                perpendicular.X2 = point.X;
                perpendicular.Y2 = point.Y;
                perpList.Add(perpendicular);
            }
            return perpList;
        }

        //-----DrawWalls Center Line on Canvas by given Start and End points
        public Line DrawCenterLine(Line line)
        {
            line.Stroke = System.Windows.Media.Brushes.Red;
            DoubleCollection dash = new DoubleCollection() { 130, 8, 15, 8 };
            line.StrokeDashArray = dash;
            return line;
        }

        //-----DrawWalls Dashed Line on Canvas by given Start and End points
        public Line DrawDashedLine(Line line)
        {
            line.Stroke = System.Windows.Media.Brushes.SteelBlue;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            DoubleCollection dash = new DoubleCollection() { 20, 5, 20, 5 };
            line.StrokeDashArray = dash;
            return line;
        }

        public double GetLength(Line line)
        {
            return Math.Sqrt(Math.Pow((line.X1 - line.X2), 2) + Math.Pow((line.Y1 - line.Y2), 2));
        }

        public double GetSlope(Line line)
        {
            return (line.Y2 - line.Y1) / (line.X2 - line.X1);
        }

        public PointF GetCenter(Line line)
        {
            PointF point = new PointF();
            point.X = (float)(line.X2 + line.X1) / 2;
            point.Y = (float)(line.Y2 + line.Y1) / 2;
            return point;
        }

        //-----Return coefficients of Line equation ax+by+c=0
        public List<double> LineEquation(Line line)
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
    }
}
