using Autodesk.Revit.UI;
using RevitFamilyBrowser.Pattern_Elements_Install;
using RevitFamilyBrowser.Revit_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;

namespace RevitFamilyBrowser.WPF_Classes
{
    /// Class Dispalys Room perimeter and allow interactions with its elements
    /// Selecting wall on canvas buld prpendiculars and detects intersection point to instal Revit elemnts
    /// 
    public partial class GridSetup : UserControl
    {
        private ExternalEvent m_ExEvent;
        private GridInstallEvent m_Handler;

        public int Scale { get; set; }
        public int CanvasSize { get; set; }
        private List<Line> WpfWalls { get; set; }
        public List<Line> BoundingBoxLines { get; set; }
        public List<Line> RevitWalls { get; set; }
        public System.Drawing.Point Derrivation { get; set; }

        private const int ExtensionLineLength = 40;
        private const int ExtensionLineExtent = 10;

        private WpfCoordinates tool = new WpfCoordinates();
        List<List<Line>> wallNormals = new List<List<Line>>();
        List<Line> RevitWallNormals = new List<Line>();
        //List<System.Drawing.Point> gridPoints = new List<System.Drawing.Point>();
        List<System.Drawing.Point> gridPoints = new List<System.Drawing.Point>();


        public GridSetup(ExternalEvent exEvent, GridInstallEvent handler)
        {
            InitializeComponent();
            m_ExEvent = exEvent;
            m_Handler = handler;

            radioEqual.IsChecked = true;
            CanvasSize = (int)this.canvas.Width;
            TextBoxSymbol.Text = " Type: " + Properties.Settings.Default.FamilySymbol;
            TextBoxFamily.Text = " Family: " + Properties.Settings.Default.FamilyName;
            ImageSymbol.Source = new BitmapImage(new Uri(GetImage()));
            comboBoxHeight.ItemsSource = Enum.GetValues(typeof(Heights));
        }

        private void buttonAddHorizontal_Click(object sender, RoutedEventArgs e)
        {
            int temp = int.Parse(TextBoxSplitPartNumber.Text);
            temp++;
            TextBoxSplitPartNumber.Text = temp.ToString();
        }

        private void buttonRemoveHorizontal_Click(object sender, RoutedEventArgs e)
        {
            int temp = int.Parse(TextBoxSplitPartNumber.Text);
            if (temp > 0)
                temp--;
            TextBoxSplitPartNumber.Text = temp.ToString();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            ClearRoomMarkup();
            RevitWallNormals.Clear();
            wallNormals.Clear();
        }

        private void ButtonInsertClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Offset = GetHeight(comboBoxHeight.Text);
            m_ExEvent.Raise();

            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close();

            TextBoxFamily.Text = string.Empty;
            TextBoxSymbol.Text = string.Empty;
        }

        private string GetImage()
        {
            string[] ImageList = Directory.GetFiles(System.IO.Path.GetTempPath() + "FamilyBrowser\\");
            string imageUri = (System.IO.Path.GetTempPath() + "FamilyBrowser\\RevitLogo.png");
            foreach (var imageName in ImageList)
            {
                if (imageName.Contains(Properties.Settings.Default.FamilySymbol))
                    imageUri = imageName;
            }
            return imageUri;
        }

        enum Heights
        {
            Socket = 200,
            Cardreader = 850,
            Lightswitch = 1100,
            Thermostat = 1300,
            FireAlarm = 1500
        }

        private int GetHeight(string text)
        {
            int height = 0;
            if (int.TryParse(text, out height))
                return height;

            if (text == ("Socket"))
                height = (int)Heights.Socket;
            else if (text == ("Cardreader"))
            {
                height = (int)Heights.Cardreader;
            }
            else if (text == ("Lightswitch"))
            {
                height = (int)Heights.Lightswitch;
            }
            else if (text == ("Thermostat"))
            {
                height = (int)Heights.Thermostat;
            }
            else if (text == ("FireAlarm"))
            {
                height = (int)Heights.FireAlarm;
            }
            else
            {
                height = 0;
            }
            return height;
        }

        public void DrawWalls()
        {

            WpfWalls = GetWpfWalls();
            foreach (Line myLine in WpfWalls)
            {
                myLine.Stroke = Brushes.Black;
                myLine.StrokeThickness = 4;

                myLine.StrokeEndLineCap = PenLineCap.Round;
                myLine.StrokeStartLineCap = PenLineCap.Round;

                myLine.MouseDown += line_MouseDown;
                myLine.MouseUp += line_MouseUp;
                myLine.MouseEnter += line_MouseEnter;
                myLine.MouseLeave += line_MouseLeave;
                myLine.ToolTip = "L = " + tool.GetLength(myLine) * Scale;
                canvas.Children.Add(myLine);
            }
        }

        #region Line interaction events

        private void line_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Line)sender).Stroke = Brushes.Gray;
        }

        private void line_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Equals(((Line)sender).Stroke, Brushes.Red))
            {
                ((Line)sender).Stroke = Brushes.Black;
            }
        }

        private void line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Line line = (Line)sender;
            line.Stroke = Brushes.Red;
            List<PointF> listPointsOnWall = GetListPointsOnWall(line, out string InstallType);
            
            List<Line> listPerpendiculars = tool.DrawPerp(line, listPointsOnWall);
            foreach (var perpendicular in listPerpendiculars)
            {
                canvas.Children.Add(tool.BuildInstallAxis(BoundingBoxLines, perpendicular));
            }

            gridPoints.Clear();
            gridPoints = tool.GetGridPoints(listPerpendiculars, wallNormals);
            AddElementsPreview();
            textBoxQuantity.Text = "Items: " + CountInstallElements();

            Dimension dimension = new Dimension();
            dimension.DrawWallDimension(line, this);
            foreach (Line item in WallPartsAfterSplit(listPointsOnWall, line))
            {
                Dimension partDim = new Dimension(30, 7, HorizontalAlignment.Center);
                partDim.DrawWallDimension(item, this);
            }

            var wallIndex = GetWallIndex(sender);
            GetRevitInstallCoordinates(RevitWallNormals, RevitWalls, wallIndex, InstallType);
        }

        private void line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Line)sender).Stroke = Brushes.Red;
        }

        #endregion

        private List<Line> GetWpfWalls()
        {
            List<Line> wpfWalls = new List<Line>();
            foreach (var item in RevitWalls)
            {
                Line myLine = new Line
                {
                    X1 = (item.X1 / Scale) + Derrivation.X,
                    Y1 = ((-item.Y1 / Scale) + Derrivation.Y),
                    X2 = (item.X2 / Scale) + Derrivation.X,
                    Y2 = ((-item.Y2 / Scale) + Derrivation.Y)
                };
                wpfWalls.Add(myLine);
            }

            return wpfWalls;
        }

        private List<PointF> GetListPointsOnWall(Line line, out string InstallType)
        {
            List<PointF> listPointsOnWall;
            WpfCoordinates wpfCoord = new WpfCoordinates();

            if (radioEqual.IsChecked == true)
            {
                listPointsOnWall = wpfCoord.SplitLineEqual(line, Convert.ToInt32(this.TextBoxSplitPartNumber.Text));
                InstallType = "Equal";
            }
            else if (radioProportoinal.IsChecked == true)
            {
                listPointsOnWall =
                    wpfCoord.SplitLineProportional(line, Convert.ToInt32(this.TextBoxSplitPartNumber.Text));
                InstallType = "Proportional";
            }
            else
            {
                double distance = (Convert.ToDouble(TextBoxDistance.Text) / Scale);
                listPointsOnWall = wpfCoord.SplitLineDistance(line, Convert.ToDouble(distance));
                InstallType = "Distance";
            }
            return listPointsOnWall;
        }

        private void TextBoxDistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (radioDistance.IsChecked == false)
                radioDistance.IsChecked = true;
        }

        private void GetRevitInstallCoordinates(List<Line> revitWallNormals, List<Line> revitWalls, int wallIndex, string InstallType)
        {
            CoordinatesRevit rvt = new CoordinatesRevit();
            Line rvtWall = revitWalls[wallIndex];

            List<PointF> rvtPointsOnWall = new List<PointF>();
            if (InstallType == "Equal")
            {
                rvtPointsOnWall = rvt.GetSplitPointsEqual(rvtWall, Convert.ToInt32(TextBoxSplitPartNumber.Text));
            }
            else if (InstallType == "Proportional")
            {
                rvtPointsOnWall = rvt.GetSplitPointsProportional(rvtWall, Convert.ToInt32(TextBoxSplitPartNumber.Text));
            }
            else if (InstallType == "Distance")
            {
                rvtPointsOnWall = rvt.GetSplitPointsDistance(rvtWall, Convert.ToInt32(TextBoxDistance.Text));
            }
            List<Line> rvtListPerpendiculars = rvt.GetPerpendiculars(rvtWall, rvtPointsOnWall);
            List<PointF> rvtGridPoints = rvt.GetGridPointsRvt(revitWallNormals, rvtListPerpendiculars);

            Properties.Settings.Default.InstallPoints = string.Empty;

            foreach (var item in rvtGridPoints)
            {
                int counter = 0;
                Line check = DrawCheckline(item, 100000000, 100000000);
                foreach (var wall in revitWalls)
                {
                    if (CheckIntersection(wall, check))
                        counter++;
                }
                if (counter % 2 != 0)
                    Properties.Settings.Default.InstallPoints += (item.X) / (25.4 * 12) + "*" + (item.Y) / (25.4 * 12) + "\n";
            }
        }

        private List<Line> WallPartsAfterSplit(List<PointF> points, Line wall)
        {
            List<Line> parts = new List<Line>();

            Line startline = new Line();
            startline.X1 = wall.X1;
            startline.Y1 = wall.Y1;
            startline.X2 = points[0].X;
            startline.Y2 = points[0].Y;
            parts.Add(startline);

            Line endLine = new Line();
            endLine.X1 = points[points.Count - 1].X;
            endLine.Y1 = points[points.Count - 1].Y;
            endLine.X2 = wall.X2;
            endLine.Y2 = wall.Y2;
            parts.Add(endLine);

            PointF pointA = new PointF();
            pointA = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                Line part = new Line();
                part.X1 = pointA.X;
                part.Y1 = pointA.Y;
                part.X2 = points[i].X;
                part.Y2 = points[i].Y;
                pointA = points[i];
                parts.Add(part);
            }
            return parts;
        }

        private int GetWallIndex(object sender)
        {
            int wallIndex = 0;
            foreach (var item in WpfWalls)
            {
                if (sender.Equals(item))
                    wallIndex = WpfWalls.IndexOf(item);
            }
            return wallIndex;
        }

        #region Elements Preview
        private void AddElementsPreview()
        {
            List<UIElement> prewiElements = canvas.Children.OfType<UIElement>().Where(n => n.Uid.Contains("ElementPreview")).ToList();
            foreach (var item in prewiElements)
            {
                canvas.Children.Remove(item);
            }
            foreach (var item in gridPoints)
            {
                int counter = 0;
                Line check = DrawCheckline(item, 0, 0);
                foreach (var wall in WpfWalls)
                {
                    if (CheckIntersection(wall, check))
                    {
                        counter++;
                    }
                }
                if (counter % 2 == 0) continue;
                Ellipse el = new Ellipse();
                el.Height = 10;
                el.Width = 10;
                el.Stroke = Brushes.Red;
                el.Fill = Brushes.White;
                el.Uid = "ElementPreview";
                Canvas.SetTop(el, item.Y - el.Height / 2);
                Canvas.SetLeft(el, item.X - el.Width / 2);
                canvas.Children.Add(el);
            }
        }

        private int CountInstallElements()
        {
            List<UIElement> prewiElements = canvas.Children.OfType<UIElement>().Where(n => n.Uid.Contains("ElementPreview")).ToList();

            int elementCount = prewiElements.Count;

            return elementCount;
        }

        private void ClearRoomMarkup()
        {
            List<Line> walls = canvas.Children.OfType<Line>().Where(w => Equals(w.Stroke, Brushes.Red)).ToList();
            foreach (var item in walls)
            {
                item.Stroke = Brushes.Black;
            }
            List<Line> lines = canvas.Children.OfType<Line>().Where(r => Equals(r.Stroke, Brushes.SteelBlue)).ToList();
            textBoxQuantity.Text = "No Items";
            foreach (var item in lines)
            {
                canvas.Children.Remove(item);
            }

            List<UIElement> dimensions = canvas.Children.OfType<UIElement>()
                .Where(n => n.Uid.Contains("Dimension"))
                .ToList();
            foreach (var item in dimensions)
            {
                canvas.Children.Remove(item);
            }

            List<UIElement> previewElements = canvas.Children.OfType<UIElement>()
                .Where(el => el.Uid.Contains("ElementPreview"))
                .ToList();
            foreach (var item in previewElements)
            {
                canvas.Children.Remove(item);
            }
        }

        private Line DrawCheckline(PointF point, int outterX, int outterY)
        {
            Line checkLine = new Line();
            checkLine.X1 = outterX;
            checkLine.Y1 = outterY;
            checkLine.X2 = point.X;
            checkLine.Y2 = point.Y;
            return checkLine;
        }

        private bool CheckIntersection(Line first, Line second)
        {
            PointF intersection = tool.GetIntersectionD(first, second);

            if ((float.IsInfinity(intersection.X)) || (float.IsInfinity(intersection.Y)))
            {
                return false;
            }

            bool belongFirst = CheckIfPointBelongToLine(first, intersection);
            bool belongSecond = CheckIfPointBelongToLine(second, intersection);

            if (belongFirst && belongSecond)
            {
                return true;
            }
            return false;
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

            double tolerance = Math.Abs(length * .00001);
            if (Math.Abs(length - summ) < tolerance)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
