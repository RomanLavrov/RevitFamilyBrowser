using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Shapes;

namespace RevitFamilyBrowser.Pattern_Elements_Install
{
    public class CoordinatesRevit
    {
        public double GetSlope(Line line)
        {
            return (line.Y2 - line.Y1) / (line.X2 - line.X1);
        }

        private double GetLength(Line line)
        {
            return Math.Sqrt(Math.Pow((line.X1 - line.X2), 2) + Math.Pow((line.Y1 - line.Y2), 2));
        }

        private List<double> GetLineEquation(Line line)
        {
            List<double> result = new List<double>();
            double a = (line.Y2 - line.Y1);
            double b = (line.X1 - line.X2);
            double c = (line.Y1 * (line.X1 - line.X2) - line.X1 * (line.Y1 - line.Y2));
            result.Add(a);
            result.Add(b);
            result.Add(-c);
            //MessageBox.Show($"{a}x + {b}y + {c}");
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

        public List<PointF> GetSplitPointsEqual(Line line, int parts)
        {
            List<PointF> splitPoints = new List<PointF>();
            int partNumber = parts + 1;

            for (int i = 1; i < partNumber; i++)
            {
                PointF point = new PointF();
                double top = i;
                double bottom = (partNumber - i);
                if (partNumber - i == 0)
                    bottom = 1;

                var proportion = top / bottom;
                point.X = (float)((line.X1 + (line.X2 * proportion)) / (1 + proportion));
                point.Y = (float)((line.Y1 + (line.Y2 * proportion)) / (1 + proportion));
                //MessageBox.Show($"Split point X={point.X}; Y={point.Y}");
                splitPoints.Add(point);
            }
            return splitPoints;
        }

        public List<PointF> GetSplitPointsProportional(Line line, int parts)
        {
            List<PointF> points = new List<PointF>();
            int partNumber = parts * 2;
            for (int i = 1; i < partNumber; i = i + 2)
            {
                PointF point = new PointF();
                float top = i;
                float bottom = (partNumber - i);
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

        #region Split wall  by distance between objects

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

        private double GetAngle(Line line)
        {
            List<double> lineCoefs = LineEquation(line);
            double angle = -Math.Atan(lineCoefs[1] / lineCoefs[0]);
            return angle;
        }

        private int GetLinePartsNumber(Line line, int distance)
        {
            int parts;
            double wallLength = this.GetLength(line);
            if ((wallLength % distance).Equals(0) && (wallLength / distance > 2))
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

        private  List<double> GetPartsSizes(Line line, int distance)
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

        private PointF GetSecondCoord(Line line, double distance)
        {
            PointF point = new Point();
            double Angle = GetAngle(line);
            point.X = (float)(line.X1 + distance * Math.Sin(Angle));
            point.Y = (float)(line.Y1 - distance * Math.Cos(Angle));
            return point;
        }
       
        public List<PointF> GetSplitPointsDistance(Line line, int distance)
        {
            List<PointF> points = new List<PointF>();
            List<double> partSizes = GetPartsSizes(line, distance);
            foreach (var part in partSizes)
            {
                points.Add(GetSecondCoord(line, part));
            }
            return points;
        }

        #endregion


        public List<Line> GetPerpendiculars(Line baseWall, List<PointF> points)
        {
            List<Line> perpList = new List<Line>();

            foreach (PointF point in points)
            {
                PointF target = new PointF();

                if (GetSlope(baseWall).CompareTo(0) == 0)
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

        public List<PointF> GetGridPointsRvt(List<Line> perpendicularsList, List<Line> perpendiculars)
        {
            List<PointF> target = new List<PointF>();
            perpendicularsList.AddRange(perpendiculars);
           
            foreach (var lineA in perpendicularsList)
            {
                foreach (var lineB in perpendicularsList)
                {
                    if (!lineA.Equals(lineB))
                        target.Add(GetIntersection(lineA, lineB));
                }
            }
            IEnumerable<PointF> distinctPoints = target.Distinct();
            List<PointF> result = new List<PointF>();
            foreach (var point in distinctPoints.ToList())
            {
                if (!float.IsInfinity(point.X) && !float.IsInfinity(point.Y) && !float.IsNaN(point.X) && !float.IsNaN(point.Y))
                {
                    result.Add(point);
                }
            }
            return result;
        }
    }
}
