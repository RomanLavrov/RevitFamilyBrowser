using System;
using RevitFamilyBrowser.WPF_Classes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace RevitFamilyBrowser.Pattern_Elements_Install
{
    public class Dimension
    {
        private const int ExtensionLineLength = 40;
        private const int ExtensionLineExtent = 10;
        private List<Point> PointList = new List<Point>();
        private List<Line> ExtensionLines = new List<Line>();
        private Line dimensionLine;
        private Point textPosition;

        public void WallSizeText(Line wall, GridSetup grid)
        {
            Label WallSize = new Label();
            WallSize.Height = 40;
            WallSize.Width = 80;
            WpfCoordinates wpfCoordinates = new WpfCoordinates();
            WallSize.Content = (int)(wpfCoordinates.GetLength(wall) * grid.Scale);
            //Canvas.SetLeft(WallSize, ((wall.X2 + wall.X1) / 2) - WallSize.Width / 2);
            //Canvas.SetTop(WallSize, ((wall.Y2 + wall.Y1) / 2) - WallSize.Height / 2 - 10);
           
            Canvas.SetLeft(WallSize, textPosition.X);
            Canvas.SetTop(WallSize, textPosition.Y);
            WallSize.LayoutTransform = new RotateTransform(SetTextAngle(wall));
            AddDimLine(wall, grid);
            grid.canvas.Children.Add(WallSize);
        }

        public void AddDimLine(Line line, GridSetup grid)
        {
            WpfCoordinates tool = new WpfCoordinates();
            tool.LineEquation(line);
            tool.GetAngle(line);

            GetWallStartEnd(line);
            GetExtensionlines(line, grid);

            dimensionLine = new Line();
            dimensionLine.X1 = tool.GetSecondCoord(ExtensionLines[0], ExtensionLineExtent).X;
            dimensionLine.Y1 = tool.GetSecondCoord(ExtensionLines[0], ExtensionLineExtent).Y;
            dimensionLine.X2 = tool.GetSecondCoord(ExtensionLines[1], ExtensionLineExtent).X;
            dimensionLine.Y2 = tool.GetSecondCoord(ExtensionLines[1], ExtensionLineExtent).Y;
            dimensionLine.Stroke = Brushes.RoyalBlue;
            textPosition = tool.GetCenter(dimensionLine);
            grid.canvas.Children.Add(dimensionLine);
        }

        private double SetTextAngle(Line line)
        {
            WpfCoordinates tool = new WpfCoordinates();
            double angleRad = tool.GetAngle(line);
            double angleDegrees = angleRad * 180 / Math.PI;

            if (angleDegrees.Equals(-90) || angleDegrees.Equals(90))
                angleDegrees = 0;
            else if (angleDegrees.Equals(0) || angleDegrees.Equals(180))
            {
                angleDegrees = -90;
            }

            return angleDegrees;
        }

        private void GetWallStartEnd(Line line)
        {
            Point start = new Point
            {
                X = (int)line.X1,
                Y = (int)line.Y1
            };
            PointList.Add(start);
            Point end = new Point
            {
                X = (int)line.X2,
                Y = (int)line.Y2
            };
            PointList.Add(end);
        }

        private void GetExtensionlines(Line line, GridSetup grid)
        {
            WpfCoordinates tool = new WpfCoordinates();
            foreach (Line item in tool.DrawPerp(line, PointList))
            {
                Line extensionLine = new Line();
                Point point = new Point();

                if (tool.GetSlope(item) > 0)
                {
                    point = tool.GetSecondCoord(item, tool.GetLength(item) - ExtensionLineLength);
                    extensionLine.X1 = point.X;
                    extensionLine.Y1 = point.Y;
                    extensionLine.X2 = item.X2;
                    extensionLine.Y2 = item.Y2;
                }
                else
                {
                    Line temp = new Line();
                    temp.X1 = item.X2;
                    temp.Y1 = item.Y2;
                    temp.X2 = 2 * item.X2 - item.X1;
                    temp.Y2 = 2 * item.Y2 - item.Y1;

                    point = tool.GetSecondCoord(temp, -ExtensionLineLength);
                    extensionLine.X1 = point.X;
                    extensionLine.Y1 = point.Y;
                    extensionLine.X2 = item.X2;
                    extensionLine.Y2 = item.Y2;
                }
                extensionLine.Stroke = Brushes.OrangeRed;
                ExtensionLines.Add(extensionLine);
                grid.canvas.Children.Add(extensionLine);
            }
        }

       }
}