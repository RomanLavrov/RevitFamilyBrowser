using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using Autodesk.Revit.UI;

namespace RevitFamilyBrowser.Revit_Classes
{
    class CoordinatesRevit
    {
        public List<PointF> GetInsertionPoints(Line wall, int parts)
        {
            List<Line> walls = new List<Line>();

            List<List<Line>> perpList = new List<List<Line>>();
            List<PointF> splitPoints = GetSplitPoints(wall, parts);
            List<Line> perpendiculars = GetPerpendiculars(wall, splitPoints);
            perpList.Add(perpendiculars);

            return null;
        }
        public double GetSlope(Line line)
        {
            return (line.Y2 - line.Y1) / (line.X2 - line.X1);
        }
        public List<double> GetLineEquation(Line line)
        {
            List<double> result = new List<double>();
            double a = (line.Y2 - line.Y1);
            double b = (line.X1 - line.X2);
            double c = (line.Y1 * (line.X1 - line.X2) - line.X1 * (line.Y1 - line.Y2));
            result.Add(a);
            result.Add(b);
            result.Add(-c);
            return result;
        }

        public PointF GetIntersection(Line line1, Line line2)
        {
            PointF intersection = new PointF();

            List<double> line1coef = GetLineEquation(line1);
            double a1 = line1coef[0];
            double b1 = line1coef[1];
            double c1 = line1coef[2];

            List<double> line2coef = GetLineEquation(line2);
            double a2 = line2coef[0];
            double b2 = line2coef[1];
            double c2 = line2coef[2];

            float x = (float)((c1 * b2 - c2 * b1) / (a2 * b1 - a1 * b2));
            float y = 0;
            if (b1.CompareTo(0) == 0)
            {
                y = (float)((-c2 - a2 * x) / b2);
            }

            else if (b2.CompareTo(0) == 0)
                y = (float)((-c1 - a1 * x) / b1);

            else
                y = (float)((-c2 - (a2 * x)) / b2);

            intersection.X = x;
            intersection.Y = y;

            return intersection;
        }

        public List<PointF> GetSplitPoints(Line line, int parts)
        {
            List<PointF> splitPoints = new List<PointF>();
            int partNumber = parts + 1;

            for (int i = 1; i < partNumber; i++)
            {
                PointF point = new PointF();
                double top = i;
                double bottom = (partNumber - i);
                if (partNumber - i == 0)
                {
                    bottom = 1;
                }

                var proportion = top / bottom;
                point.X = (float)((line.X1 + (line.X2 * proportion)) / (1 + proportion));
                point.Y = (float)((line.Y1 + (line.Y2 * proportion)) / (1 + proportion));
                splitPoints.Add(point);
            }
            return splitPoints;
        }

        public List<Line> GetPerpendiculars(Line baseWall, List<PointF> points)
        {
            List<Line> lines = new List<Line>();
            double slope = -1 / GetSlope(baseWall);

            foreach (PointF point in points)
            {
                PointF target = new PointF();
                target.X = 0;
                target.Y = (float)(point.Y - (slope * point.X));

                Line perpendicular = new Line();
                perpendicular.X1 = target.X;
                perpendicular.Y1 = !float.IsInfinity(target.Y) ? target.Y : point.Y;
                perpendicular.X2 = point.X;
                perpendicular.Y2 = point.Y;
                lines.Add(perpendicular);
            }
            return lines;
        }

        public List<PointF> GetGridPointsRvt(List<Line> perpendicularsList, List<Line> perpendiculars)
        {
            List<PointF> target = new List<PointF>();
            perpendicularsList.AddRange(perpendiculars);
            Console.WriteLine(perpendicularsList.Count);
            foreach (var lineA in perpendicularsList)
            {
                foreach (var lineB in perpendicularsList)
                {
                    if (lineA.Equals(lineB))
                        ;//Console.WriteLine("SameLine");
                    else
                    {
                        target.Add(GetIntersection(lineA, lineB));
                    }
                }
            }
            IEnumerable<PointF> distinctPoints = target.Distinct();
            List<PointF> result = new List<PointF>();
            foreach (var point in distinctPoints.ToList())
            {
                if (!float.IsInfinity(point.X) && !float.IsInfinity(point.Y))
                {
                    result.Add(point);
                }
            }
            return result;
        }

        //public Line OrtoNormalization(Line perpend)
        //{
        //    Line normal = new Line();
        //    double xA = 0;
        //    double xB = 0;
        //    double yA = 0;
        //    double yB = 0;

        //    if (perpend.X1 != double.MaxValue || perpend.X1 != double.MinValue)
        //    {
        //        xA = perpend.X1;
        //    }
        //    if (perpend.Y1 != double.MaxValue || perpend.Y1 != double.MinValue)
        //    {
        //        yA = perpend.Y1;
        //    }
        //    if (perpend.X2 != double.MaxValue || perpend.X2 != double.MinValue)
        //    {
        //        xB = perpend.X2;
        //    }
        //    if (perpend.Y2 != double.MaxValue || perpend.Y2 != double.MinValue)
        //    {
        //        yB = perpend.Y2;
        //    }

        //    if ((xA == double.MaxValue || xA == double.MinValue) && yA == 0)
        //    {
        //        yA = 0; xA = xB;
        //    }

        //    if ((xB == double.MaxValue || xB == double.MinValue) && yB == 0)
        //    {
        //        xB = 0; yB = yA;
        //    }

        //    if ((yA == double.MaxValue || yA == double.MinValue) && xA == 0)
        //    {
        //        yA = 0; xA = xB;
        //    }

        //    if ((yB == double.MaxValue || yB == double.MinValue) && xB == 0)
        //    {
        //        yB = 0; xB = xA;
        //    }
        //    normal.X1 = xA;
        //    normal.X2 = xB;
        //    normal.Y1 = yA;
        //    normal.Y2 = yB;

        //    return normal;
        //}
    }
}
