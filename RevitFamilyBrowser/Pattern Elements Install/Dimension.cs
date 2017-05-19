using System;
using RevitFamilyBrowser.WPF_Classes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Effects;
using Autodesk.Revit.UI;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace RevitFamilyBrowser.Pattern_Elements_Install
{
    public class Dimension
    {
        private const int ExtensionLineLength = 60;
        private const int ExtensionLineExtent = 7;
        private readonly SolidColorBrush lineColor = Brushes.DarkSlateBlue;
        private List<PointF> PointList = new List<PointF>();
        private List<Line> ExtensionLines = new List<Line>();
        private Line dimensionLine;

        public void DrawWallDimension(Line wall, GridSetup grid)
        {
            Label wallSize = new Label
            {
                Width = 100,
                Height = 25,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                FontSize = 14,
                Foreground = lineColor,
                Effect = null
            };

            WpfCoordinates wpfCoordinates = new WpfCoordinates();
            wallSize.Content = Math.Round((wpfCoordinates.GetLength(wall) * grid.Scale)+0.05).ToString();

            DrawDimLine(wall, grid);
            wallSize.RenderTransform = new RotateTransform(270 - SetTextAngle(wall), wallSize.Width / 2, wallSize.Height);

            Point pos = GetTextposition();
            Canvas.SetLeft(wallSize, pos.X - wallSize.Width / 2);
            Canvas.SetTop(wallSize, pos.Y - wallSize.Height);

            grid.canvas.Children.Add(wallSize);
        }

        public void DrawDimLine(Line line, GridSetup grid)
        {
            WpfCoordinates tool = new WpfCoordinates();
            tool.LineEquation(line);
            tool.GetAngle(line);

            GetWallStartEnd(line);
            DrawExtensionLines(line, grid);

            dimensionLine = new Line();
            dimensionLine.X1 = tool.GetSecondCoord(ExtensionLines[0], ExtensionLineExtent).X;
            dimensionLine.Y1 = tool.GetSecondCoord(ExtensionLines[0], ExtensionLineExtent).Y;
            dimensionLine.X2 = tool.GetSecondCoord(ExtensionLines[1], ExtensionLineExtent).X;
            dimensionLine.Y2 = tool.GetSecondCoord(ExtensionLines[1], ExtensionLineExtent).Y;
            dimensionLine.Stroke = lineColor;

            DrawDimensionTick(dimensionLine, grid);
            grid.canvas.Children.Add(dimensionLine);
        }

        private double SetTextAngle(Line line)
        {
            WpfCoordinates tool = new WpfCoordinates();
            double angleRad = tool.GetAngle(line);
            double angleDegrees = (angleRad * 180 / Math.PI);
            if (angleDegrees > 0 && angleDegrees <= 90)
            {
                angleDegrees = angleDegrees + 180;
            }
            return angleDegrees;
        }

        private void GetWallStartEnd(Line line)
        {
            PointF start = new PointF()
            {
                X = (float)line.X1,
                Y = (float)line.Y1
            };
            PointList.Add(start);
            PointF end = new PointF
            {
                X = (float)line.X2,
                Y = (float)line.Y2
            };
            PointList.Add(end);
        }

        private void DrawExtensionLines(Line line, GridSetup grid)
        {
            WpfCoordinates tool = new WpfCoordinates();
            foreach (Line item in tool.DrawPerp(line, PointList))
            {
                Line extensionLine = new Line();
                PointF point = new PointF();

                if (tool.GetSlope(item) > 0)
                {
                    point = tool.GetSecondCoord(item, tool.GetLength(item) - ExtensionLineLength);
                }
                else
                {
                    Line temp = new Line();
                    temp.X1 = item.X2;
                    temp.Y1 = item.Y2;
                    temp.X2 = 2 * item.X2 - item.X1;
                    temp.Y2 = 2 * item.Y2 - item.Y1;

                    point = tool.GetSecondCoord(temp, -ExtensionLineLength);
                }

                extensionLine.X1 = point.X;
                extensionLine.Y1 = point.Y;
                extensionLine.X2 = item.X2;
                extensionLine.Y2 = item.Y2;

                extensionLine.Stroke = lineColor;
                ExtensionLines.Add(extensionLine);
                grid.canvas.Children.Add(extensionLine);
            }
        }

        private Point GetTextposition()
        {
            WpfCoordinates tool = new WpfCoordinates();
            return tool.GetCenter(this.dimensionLine);
        }

        private void DrawDimensionTick(Line line, GridSetup grid)
        {
            WpfCoordinates tool = new WpfCoordinates();
            double angle = tool.GetAngle(line);
            int length = 7;
            double tickAngle = angle + 45 * Math.PI / 180;

            Line leftTick = new Line
            {
                X1 = line.X1 - length * Math.Sin(tickAngle),
                Y1 = line.Y1 - length * Math.Cos(tickAngle),
                X2 = line.X1 + length * Math.Sin(tickAngle),
                Y2 = line.Y1 + length * Math.Cos(tickAngle),
                Stroke = lineColor,
                StrokeThickness = 2
            };

            Line rightTick = new Line
            {
                X1 = line.X2 - length * Math.Sin(tickAngle),
                Y1 = line.Y2 - length * Math.Cos(tickAngle),
                X2 = line.X2 + length * Math.Sin(tickAngle),
                Y2 = line.Y2 + length * Math.Cos(tickAngle),
                Stroke = lineColor,
                StrokeThickness = 2
            };

            grid.canvas.Children.Add(leftTick);
            grid.canvas.Children.Add(rightTick);
        }

    }
}