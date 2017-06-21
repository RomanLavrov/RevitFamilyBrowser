using System;
using RevitFamilyBrowser.WPF_Classes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace RevitFamilyBrowser.Pattern_Elements_Install
{
    public class Dimension
    {
        private int ExtensionLineLength;
        private int ExtensionLineExtent;
        private HorizontalAlignment allign;
        private Line dimensionLine;
        private SolidColorBrush lineColor = Brushes.DarkSlateBlue;

        private List<PointF> PointList = new List<PointF>();
        private List<Line> ExtensionLines = new List<Line>();
        private List<UIElement> interfaceElements = new List<UIElement>();
        private WpfCoordinates tool = new WpfCoordinates();

        public Dimension()
        {
            ExtensionLineLength = 60;
            ExtensionLineExtent = 7;
            allign = HorizontalAlignment.Right;
        }

        public Dimension(int ExtensionlineLength, int ExtensionlineExtent, HorizontalAlignment allign)
        {
            ExtensionLineLength = ExtensionlineLength;
            ExtensionLineExtent = ExtensionlineExtent;
            this.allign = allign;
        }

        public void DrawWallDimension(Line wall, GridSetup grid)
        {
            AddExtensionLines(wall);
            AddDimLine(wall);
            AddDimensionTick(dimensionLine);
            AddAnnotation(wall, grid);

            DrawDimension(grid);
        }

        private void AddDimLine(Line line)
        {
            tool.LineEquation(line);
            tool.GetAngle(line);
            GetWallStartEnd(line);

            dimensionLine = new Line();
            dimensionLine.X1 = tool.GetSecondCoord(ExtensionLines[0], ExtensionLineExtent).X;
            dimensionLine.Y1 = tool.GetSecondCoord(ExtensionLines[0], ExtensionLineExtent).Y;
            dimensionLine.X2 = tool.GetSecondCoord(ExtensionLines[1], ExtensionLineExtent).X;
            dimensionLine.Y2 = tool.GetSecondCoord(ExtensionLines[1], ExtensionLineExtent).Y;
            dimensionLine.Stroke = lineColor;

            interfaceElements.Add(dimensionLine);
        }
       
        private void AddAnnotation(Line wall, GridSetup grid)
        {
            Label wallSize = new Label
            {
                Width = 100,
                Height = 25,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                HorizontalContentAlignment = allign,
                FontSize = 14,
                Foreground = lineColor,
                Effect = null,
                Content = Math.Round((tool.GetLength(wall) * grid.Scale) + 0.05).ToString()
            };

            wallSize.RenderTransform = new RotateTransform(270 - SetTextAngle(wall), wallSize.Width / 2, wallSize.Height);

            PointF textPosition = tool.GetCenter(dimensionLine);
            Canvas.SetLeft(wallSize, textPosition.X - wallSize.Width / 2);
            Canvas.SetTop(wallSize, textPosition.Y - wallSize.Height);
            wallSize.Uid = "Dimension";
            interfaceElements.Add(wallSize);
        }

        private double SetTextAngle(Line line)
        {
            double angleRad = tool.GetAngle(line);
            double angleDegrees = (angleRad * 180 / Math.PI);
            if (angleDegrees > 6 && angleDegrees <= 90)
            {
                angleDegrees = angleDegrees + 180;
            }
            return angleDegrees;
        }

        private void AddExtensionLines(Line line)
        {
            GetWallStartEnd(line);
            foreach (Line item in tool.GetPerpendiculars(line, PointList))
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
                extensionLine.Uid = "DimensionExtent";
                extensionLine.Stroke = lineColor;

                ExtensionLines.Add(extensionLine);
                interfaceElements.Add(extensionLine);
            }
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

        private void AddDimensionTick(Line line)
        {
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
            interfaceElements.Add(leftTick);
            interfaceElements.Add(rightTick);
        }

        private void DrawDimension(GridSetup grid)
        {
            foreach (var item in interfaceElements)
            {
                item.Uid = "Dimension";
                grid.canvas.Children.Add(item);
            }
        }
    }
}