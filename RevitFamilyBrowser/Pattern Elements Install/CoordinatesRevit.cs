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
                    bottom = 1;

                var proportion = top / bottom;
                point.X = (float)((line.X1 + (line.X2 * proportion)) / (1 + proportion));
                point.Y = (float)((line.Y1 + (line.Y2 * proportion)) / (1 + proportion));
                //MessageBox.Show($"Split point X={point.X}; Y={point.Y}");
                splitPoints.Add(point);
            }
            return splitPoints;
        }

        public List<Line> GetPerpendiculars(Line baseWall, List<PointF> points)
        {
            List<Line> perpList = new List<Line>();

            for (var index = 0; index < points.Count; index++)
            {
                PointF point = points[index];
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
                    target.Y = (float) (point.Y - (slope * point.X));
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
            //MessageBox.Show(perpendicularsList.Count.ToString());
            foreach (var lineA in perpendicularsList)
            {
                foreach (var lineB in perpendicularsList)
                {
                    //MessageBox.Show(String.Format("Start x={0}; y={1};\n End x={2}; y={3}\n", lineB.X1, lineB.Y1, lineB.X2, lineB.Y2));
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
