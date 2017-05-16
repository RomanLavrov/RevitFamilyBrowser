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
        public int Scale { get; set; }
        public int CanvasSize { get; set; }
        private ExternalEvent m_ExEvent;
        private GridInstallEvent m_Handler;
        public List<Line> WpfWalls { get; set; }
        public List<Line> BoundingBoxLines { get; set; }
        public List<Line> RevitWalls { get; set; }
        public System.Drawing.Point Derrivation { get; set; }
        private const int ExtensionLineLength = 40;
        private const int ExtensionLineExtent = 10;

        List<Line> RevitWallNormals = new List<Line>();
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

        public void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            List<System.Windows.Shapes.Line> lines = canvas.Children.OfType<System.Windows.Shapes.Line>().Where(r => Equals(r.Stroke, Brushes.SteelBlue)).ToList();
            textBoxQuantity.Text = "No Items";
            foreach (var item in lines)
            {
                canvas.Children.Remove(item);
            }
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

        public void Draw()
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
                canvas.Children.Add(myLine);
            }
        }

        private void line_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Equals(((Line)sender).Stroke, Brushes.Red))
            {
                ((System.Windows.Shapes.Line)sender).Stroke = Brushes.Black;
            }
        }

        private void line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Change line colour back to normal 
            ((Line)sender).Stroke = Brushes.Red;
        }

        private void line_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Line)sender).Stroke = Brushes.Gray;
            //------------------------------------Add Wall size---------------------------------------
            //Line line = sender as Line;
            //WpfCoordinates wpfCoord = new WpfCoordinates();
            //System.Windows.Controls.Label WallDimension = new Label();
            //WallDimension.Content = wpfCoord.GetLength(line).ToString();
            //Canvas.SetLeft(WallDimension, line.X2 + wpfCoord.GetLength(line)/2);
            //Canvas.SetTop(WallDimension, line.Y2 + wpfCoord.GetLength(line) / 2);
            //this.canvas.Children.Add(WallDimension);
            //-----------------------------------------------------------------------------
        }

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

            foreach (var item in rvtGridPoints)
            {
                Properties.Settings.Default.InstallPoints += (item.X) / (25.4 * 12) + "*" + (item.Y) / (25.4 * 12) + "\n";
            }
        }

        private List<System.Drawing.Point> GetListPointsOnWall(Line line, out string InstallType)
        {
            List<System.Drawing.Point> listPointsOnWall;
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
                listPointsOnWall = wpfCoord.SplitLineDistance(line, Convert.ToInt32(distance));
                InstallType = "Distance";
            }
            return listPointsOnWall;
        }

        private void TextBoxDistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (radioDistance.IsChecked == false)
                radioDistance.IsChecked = true;
        }

        private void line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Line line = (Line)sender;
            line.Stroke = Brushes.Red;

            int wallIndex = 0;
            foreach (var item in WpfWalls)
            {
                if (sender.Equals(item))
                    wallIndex = WpfWalls.IndexOf(item);
            }

            List<System.Drawing.Point> listPointsOnWall = GetListPointsOnWall(line, out string InstallType);
            Dimension dimension = new Dimension();
            dimension.WallSizeText(line, this);
            dimension.AddDimLine(line, this);
            WpfCoordinates wpfCoord = new WpfCoordinates();
            List<Line> listPerpendiculars = wpfCoord.DrawPerp(line, listPointsOnWall);
            foreach (var perpendicular in listPerpendiculars)
            {
                canvas.Children.Add(wpfCoord.BuildBoundedLine(BoundingBoxLines, perpendicular));
                // AddSegmentSize(line, perpendicular);
            }
            gridPoints.Clear();

            gridPoints = wpfCoord.GetGridPoints(listPerpendiculars);
            textBoxQuantity.Text = "Items: " + gridPoints.Count;
            GetRevitInstallCoordinates(RevitWallNormals, RevitWalls, wallIndex, InstallType);
        }

        private void AddSegmentSize(Line wall, Line perpendicular)
        {
            WpfCoordinates wpfCoord = new WpfCoordinates();
            Label WallSegmentSize = new Label();
            WallSegmentSize.Content = (int)(wpfCoord.GetLength(wall) * Scale);
            Canvas.SetLeft(WallSegmentSize, perpendicular.X2);
            Canvas.SetTop(WallSegmentSize, perpendicular.Y2);
            canvas.Children.Add(WallSegmentSize);
        }

        //private void WallSizeText(Line wall)
        //{
        //    Label WallSize = new Label();
        //    WallSize.Height = 40;
        //    WallSize.Width = 80;
        //    WpfCoordinates wpfCoordinates = new WpfCoordinates();
        //    WallSize.Content = (int)(wpfCoordinates.GetLength(wall) * Scale);
        //    Canvas.SetLeft(WallSize, ((wall.X2 + wall.X1) / 2) - WallSize.Width / 2);
        //    Canvas.SetTop(WallSize, ((wall.Y2 + wall.Y1) / 2) - WallSize.Height / 2 - 10);
        //    WallSize.LayoutTransform = new RotateTransform(SetTextAngle(wall));
        //    AddDimLine(wall);
        //    canvas.Children.Add(WallSize);
        //}

        //private void AddDimLine(Line line)
        //{
        //    WpfCoordinates tool = new WpfCoordinates();
        //    tool.LineEquation(line);
        //    tool.GetAngle(line);

        //    List<System.Drawing.Point> pointList = new List<System.Drawing.Point>();
        //    System.Drawing.Point start = new System.Drawing.Point
        //    {
        //        X = (int)line.X1,
        //        Y = (int)line.Y1
        //    };
        //    pointList.Add(start);
        //    System.Drawing.Point end = new System.Drawing.Point
        //    {
        //        X = (int)line.X2,
        //        Y = (int)line.Y2
        //    };
        //    pointList.Add(end);
          
        //    List<Line> extensionLines = new List<Line>();

        //    foreach (Line item in tool.DrawPerp(line, pointList))
        //    {
        //        Line extensionLine = new Line();
        //        System.Drawing.Point point = new System.Drawing.Point();

        //        if (tool.GetSlope(item) > 0)
        //        {
        //            point = tool.GetSecondCoord(item, tool.GetLength(item) - ExtensionLineLength);
        //            extensionLine.X1 = point.X;
        //            extensionLine.Y1 = point.Y;
        //            extensionLine.X2 = item.X2;
        //            extensionLine.Y2 = item.Y2;
        //        }
        //        else
        //        {
        //            Line temp = new Line();
        //            temp.X1 = item.X2;
        //            temp.Y1 = item.Y2;
        //            temp.X2 = 2 * item.X2 - item.X1;
        //            temp.Y2 = 2 * item.Y2 - item.Y1;
                    
        //            point = tool.GetSecondCoord(temp, - ExtensionLineLength);
        //            extensionLine.X1 = point.X;
        //            extensionLine.Y1 = point.Y;
        //            extensionLine.X2 = item.X2;
        //            extensionLine.Y2 = item.Y2;
        //        }
        //        extensionLine.Stroke = Brushes.OrangeRed;
        //        extensionLines.Add(extensionLine);
        //        canvas.Children.Add(extensionLine);
        //    }

        //    Line dimensionLine = new Line();
        //    dimensionLine.X1 = tool.GetSecondCoord(extensionLines[0], ExtensionLineExtent).X;
        //    dimensionLine.Y1 = tool.GetSecondCoord(extensionLines[0], ExtensionLineExtent).Y;
        //    dimensionLine.X2 = tool.GetSecondCoord(extensionLines[1], ExtensionLineExtent).X;
        //    dimensionLine.Y2 = tool.GetSecondCoord(extensionLines[1], ExtensionLineExtent).Y;
        //    dimensionLine.Stroke = Brushes.RoyalBlue;
        //    canvas.Children.Add(dimensionLine);

        //}

        //private double SetTextAngle(Line line)
        //{
        //    WpfCoordinates tool = new WpfCoordinates();
        //    double angleRad = tool.GetAngle(line);
        //    double angleDegrees = angleRad * 180 / Math.PI;

        //    if (angleDegrees.Equals(-90) || angleDegrees.Equals(90))
        //        angleDegrees = 0;
        //    else if (angleDegrees.Equals(0) || angleDegrees.Equals(180))
        //    {
        //        angleDegrees = -90;
        //    }

        //    return angleDegrees;
        //}
    }
}
