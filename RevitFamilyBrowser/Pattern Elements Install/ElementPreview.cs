using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitFamilyBrowser.WPF_Classes;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Brushes = System.Windows.Media.Brushes;
using Ellipse = System.Windows.Shapes.Ellipse;
using Line = System.Windows.Shapes.Line;

namespace RevitFamilyBrowser.Pattern_Elements_Install
{
    class ElementPreview
    {
        private readonly WpfCoordinates tool = new WpfCoordinates();
        private int wpfOutterCheckpoint = 0;
        
        public List<PointF> AddElementsPreviewF(GridSetup grid)
        {
            List<PointF> filteredPoints = new List<PointF>();
            List<UIElement> prewiElements = grid.canvas.Children.OfType<UIElement>().Where(n => n.Uid.Contains("ElementPreview")).ToList();
            foreach (var item in prewiElements)
            {
                grid.canvas.Children.Remove(item);
            }
            foreach (var item in grid.gridPoints)
            {
                int counter = 0;
                Line check = DrawCheckline(item, wpfOutterCheckpoint, wpfOutterCheckpoint);

                foreach (var wall in grid.WpfWalls)
                {
                    PointF intersection = tool.GetIntersection(check, wall);
                    if (CheckIfPointBelongToLine(check, intersection))
                    {
                        if (CheckIfPointBelongToLine(wall, intersection))
                            counter++;
                    }
                }
               
                if (counter % 2 == 0) continue;

                Ellipse el = new Ellipse();
                el.Height = 10;
                el.Width = 10;
                el.Stroke = Brushes.Tomato;
                el.Fill = Brushes.Aqua;
                el.Uid = "ElementPreview";
                Canvas.SetTop(el, item.Y - el.Height / 2);
                Canvas.SetLeft(el, item.X - el.Width / 2);
                filteredPoints.Add(item);
                grid.canvas.Children.Add(el);
            }

            return filteredPoints;
        }

        private Line DrawCheckline(PointF point, double outterX, double outterY)
        {
            Line checkLine = new Line();
            checkLine.X1 = outterX;
            checkLine.Y1 = outterY;
            checkLine.X2 = point.X;
            checkLine.Y2 = point.Y;
          
            return checkLine;
        }
        
        bool CheckIfPointBelongToLine(Line line, PointF point)
        {
            Line check1 = new Line();
            check1.X1 = line.X1;
            check1.Y1 = line.Y1;
            check1.X2 = point.X;
            check1.Y2 = point.Y;

            Line check2 = new Line();
            check2.X1 = line.X2;
            check2.Y1 = line.Y2;
            check2.X2 = point.X;
            check2.Y2 = point.Y;

            double summ = tool.GetLength(check1) + tool.GetLength(check2);
           
            double length = tool.GetLength(line);
           
            double tolerance = 0.00001;
            if (Math.Abs(length - summ) < tolerance)
            {
                return true;
            }
            return false;
        }

    }
}
