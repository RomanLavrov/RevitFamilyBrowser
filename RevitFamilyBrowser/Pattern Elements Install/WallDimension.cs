using System;
using RevitFamilyBrowser.WPF_Classes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media.Effects;
using Autodesk.Revit.UI;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace RevitFamilyBrowser.Pattern_Elements_Install
{
    public class Dimension
    {
        private  int ExtensionLineLength = 60;
        private  int ExtensionLineExtent = 7;
        private HorizontalAlignment allign = HorizontalAlignment.Right;
        private readonly SolidColorBrush lineColor = Brushes.DarkSlateBlue;
        private List<PointF> PointList = new List<PointF>();
        private List<Line> ExtensionLines = new List<Line>();
        private Line dimensionLine;
        private List<UIElement> interfaceElements = new List<UIElement>();

        public Dimension()
        {
            
        }
        public Dimension(int ExtensionlineLength, int ExtensionlineExtent, HorizontalAlignment allign)
        {
            this.ExtensionLineLength = ExtensionlineLength;
            this.ExtensionLineExtent = ExtensionlineExtent;
            this.allign = allign;
        }
        public void DrawWallDimension(Line wall, GridSetup grid)
        {
            Label wallSize = new Label
            {
                Width = 100,
                Height = 25,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                HorizontalContentAlignment = allign,
                FontSize = 14,
                Foreground = lineColor,
                Effect = null
            };

            WpfCoordinates wpfCoordinates = new WpfCoordinates();
            wallSize.Content = Math.Round((wpfCoordinates.GetLength(wall) * grid.Scale) + 0.05).ToString();

            DrawDimLine(wall, grid);
            wallSize.RenderTransform = new RotateTransform(270 - SetTextAngle(wall), wallSize.Width / 2, wallSize.Height);

            Point pos = GetTextposition();
            Canvas.SetLeft(wallSize, pos.X - wallSize.Width / 2);
            Canvas.SetTop(wallSize, pos.Y - wallSize.Height);
            wallSize.Uid = "Dimension";
            interfaceElements.Add(wallSize);
            drawChildrens(grid);
           // grid.canvas.Children.Add(wallSize);
        }

        private void DrawDimLine(Line line, GridSetup grid)
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
            interfaceElements.Add(dimensionLine);
           // grid.canvas.Children.Add(dimensionLine);
        }

        private double SetTextAngle(Line line)
        {
            WpfCoordinates tool = new WpfCoordinates();
            double angleRad = tool.GetAngle(line);
            double angleDegrees = (angleRad * 180 / Math.PI);
            if (angleDegrees > 5 && angleDegrees <= 90)
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
                extensionLine.Uid = "DimensionExtent";
                extensionLine.Stroke = lineColor;
                ExtensionLines.Add(extensionLine);
                interfaceElements.Add(extensionLine);
              //  grid.canvas.Children.Add(extensionLine);
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
            interfaceElements.Add(leftTick);
            interfaceElements.Add(rightTick);
            //grid.canvas.Children.Add(leftTick);
            //grid.canvas.Children.Add(rightTick);
        }

        public List<Line> GetPartials(List<PointF> points, Line wall, GridSetup grid)
        {
            List<Line> parts = new List<Line>();
            List<PointF> partCoordinates = new List<PointF>();

            PointF start = new PointF();
            start.X = (float)wall.X1;
            start.Y = (float)wall.Y1;
            partCoordinates.Add(start);

            PointF end = new PointF();
            end.X = (float)wall.X2;
            end.Y = (float)wall.Y2;
            partCoordinates.Add(end);

            partCoordinates.AddRange(points);
            partCoordinates.OrderByDescending(p => p.X).ToList();
            string temp = string.Empty;
            foreach (PointF point  in partCoordinates)
            {
                temp +=(point.X + " - " + point.Y);
            }
            MessageBox.Show(temp);
            temp = string.Empty;
            partCoordinates.Reverse();
            foreach (PointF point in partCoordinates)
            {
                temp += (point.X + " - " + point.Y);
            }
            MessageBox.Show(temp);

            PointF pointA = partCoordinates[0];
            for (int i = 1; i < partCoordinates.Count; i++)
            {
                Line part = new Line();
                part.X1 = pointA.X;
                part.Y1 = pointA.Y;
                part.X2 = partCoordinates[i].X;
                part.Y2 = partCoordinates[i].Y;
                pointA = partCoordinates[i];
                parts.Add(part);
            }
            return parts;
        }

        private void DrawPartsDimensions(Line line, GridSetup grid)
        {
            Label partSize = new Label
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
            partSize.Content = Math.Round((wpfCoordinates.GetLength(line) * grid.Scale) + 0.05).ToString();
        }

        private void drawChildrens(GridSetup grid)
        {
            foreach (var item in interfaceElements)
            {
                item.Uid = "Dimension";
                grid.canvas.Children.Add(item);
            }
        }
    }
}