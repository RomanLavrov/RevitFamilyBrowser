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
    public class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Xstart { get; set; }
        public int Ystart { get; set; }
        public int Xend { get; set; }
        public int Yend { get; set; }

        public double GetLength(Line line)
        {
            return Math.Sqrt(Math.Pow((line.X1 - line.X2), 2) + Math.Pow((line.Y1 - line.Y1), 2));
        }

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

        public List<Point> SplitLine(Line line, int lineNumber)
        {
            List<Point> points = new List<Point>();
            double length = GetLength(line);
            int partNumber = lineNumber + 1;
            double partLength = length / (lineNumber + 1);

            for (int i = 1; i < partNumber; i++)
            {
                Point point = new Point();
                point.X = (int)((line.X1 + line.X2) / (partNumber) * i);
                point.Y = (int)((line.Y1 + line.Y2) / (partNumber) * i);
                points.Add(point);
            }
            return points;
        }

        public Line SetLineLength(Line line, int length)
        {
            Point end = new Point();
            end.X = 0;
            end.Y = 0;

            for (int x = int.MinValue; x < int.MaxValue; x++)
            {
                for (int y = int.MinValue; y < int.MaxValue; y++)
                {
                    if (length == Math.Sqrt(Math.Pow((line.X1 - x), 2) + Math.Pow((line.Y2 - y), 2)))
                    {
                        end.X = x;
                        end.Y = y;
                    }
                }
            }
            line.X2 = end.X;
            line.Y2 = end.Y;
            return line;
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

        public Line DrawPerpandicularA(Line line)
        {
            Point center = this.GetCenter(line);
            double slope = -1 / (GetSlope(line));
            Point target = new Point();
            target.X = 0;
            target.Y = (int)(center.Y - (slope * center.X));

            Line perpendicular = new Line();
            perpendicular.X1 = target.X;
            perpendicular.Y1 = target.Y;
            perpendicular.X2 = center.X;
            perpendicular.Y2 = center.Y;
            perpendicular.Stroke = System.Windows.Media.Brushes.Red;
            //  DrawDashedLine(perpendicular);
            return perpendicular;
        }

        public Line DrawPerpandicularB(Line line)
        {
            Point center = this.GetCenter(line);
            double slope = -1 / (GetSlope(line));
            Point target = new Point();
            target.X = (int)(center.X - (center.Y / slope));
            target.Y = 0;

            Line perpendicular = new Line();
            perpendicular.X1 = center.X;
            perpendicular.Y1 = center.Y;
            perpendicular.X2 = target.X;
            perpendicular.Y2 = target.Y;
            perpendicular.Stroke = System.Windows.Media.Brushes.Red;
            // DrawCenterLine(perpendicular);
            return perpendicular;
        }

        //-----Draw Center Line on Canvas by given Start and End points
        public System.Windows.Shapes.Line DrawCenterLine(System.Windows.Shapes.Line line)
        {
            line.Stroke = System.Windows.Media.Brushes.Red;
            DoubleCollection dash = new DoubleCollection() { 130, 8, 15, 8 };
            line.StrokeDashArray = dash;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            return line;
        }

        //-----Draw Dashed Line on Canvas by given Start and End points
        public System.Windows.Shapes.Line DrawDashedLine(System.Windows.Shapes.Line line)
        {
            line.Stroke = System.Windows.Media.Brushes.Gray;
            DoubleCollection dash = new DoubleCollection() { 12, 12 };
            line.StrokeDashArray = dash;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            return line;
        }
    }
}
