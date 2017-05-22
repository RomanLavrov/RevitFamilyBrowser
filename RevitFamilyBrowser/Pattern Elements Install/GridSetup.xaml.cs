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

        List<List<Line>> wallNormals = new List<List<Line>>();



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

            List<PointF> listPointsOnWall = GetListPointsOnWall(line, out string InstallType);

            WallDimension wallDimension = new WallDimension();
            wallDimension.DrawWallDimension(line, this);
           // wallDimension.DrawDimLine(line, this);

            WpfCoordinates wpfCoord = new WpfCoordinates();
            
            List<Line> listPerpendiculars = wpfCoord.DrawPerp(line, listPointsOnWall);
            foreach (var perpendicular in listPerpendiculars)
            {
                canvas.Children.Add(wpfCoord.BuildBoundedLine(BoundingBoxLines, perpendicular));
            }
            gridPoints.Clear();
            gridPoints = wpfCoord.GetGridPoints(listPerpendiculars, wallNormals);


            foreach (Line item in GetPartials(listPointsOnWall, line, this))
            {
                WallDimension partDim = new WallDimension(30, 7, HorizontalAlignment.Center);
                partDim.DrawWallDimension(item, this);
            }
           
            textBoxQuantity.Text = "Items: " + gridPoints.Count;
            GetRevitInstallCoordinates(RevitWallNormals, RevitWalls, wallIndex, InstallType);
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
                Properties.Settings.Default.InstallPoints += (item.X) / (25.4 * 12) + "*" + (item.Y) / (25.4 * 12) + "\n";
            }
        }

        public List<Line> GetPartials(List<PointF> points, Line wall, GridSetup grid)
        {
            List<Line> parts = new List<Line>();
            List<PointF> partCoordinates = new List<PointF>();

            PointF start = new PointF();
            start.X = (float)wall.X1;
            start.Y = (float)wall.Y1;
           // partCoordinates.Add(start);

            PointF end = new PointF();
            end.X = (float)wall.X2;
            end.Y = (float)wall.Y2;
           // partCoordinates.Add(end);

            Line startline = new Line();
            startline.X1 = start.X;
            startline.Y1 = start.Y;
            startline.X2 = points[0].X;
            startline.Y2 = points[0].Y;
            parts.Add(startline);

            Line endLine = new Line();
            endLine.X1 = points[points.Count-1].X;
            endLine.Y1 = points[points.Count-1].Y;
            endLine.X2 = end.X;
            endLine.Y2 = end.Y;
            parts.Add(endLine);


           // partCoordinates.AddRange(points);
           // partCoordinates.OrderByDescending(p => p.X).ToList();
       
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
    }
}
