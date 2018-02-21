using Autodesk.Revit.UI;
using zRevitFamilyBrowser.Pattern_Elements_Install;
using zRevitFamilyBrowser.Revit_Classes;
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
using Autodesk.Revit.DB.Architecture;
using Brushes = System.Windows.Media.Brushes;

namespace zRevitFamilyBrowser.WPF_Classes
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
        public List<Line> WpfWalls { get; set; }
        public List<Line> BoundingBoxLines { get; set; }
        public List<Line> RevitWalls { get; set; }
        public PointF Derrivation { get; set; }
        public Room Room { get; set; }

        private const int ExtensionLineLength = 40;
        private const int ExtensionLineExtent = 10;

        private WpfCoordinates tool = new WpfCoordinates();

        List<Line> wallNormals = new List<Line>();
        List<Line> RevitWallNormals = new List<Line>();

        //public List<System.Drawing.Point> gridPoints = new List<System.Drawing.Point>();
        public List<PointF> gridPoints = new List<PointF>();

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
            List<PointF> listPointsOnWall = GetListPointsOnWall(line);

            List<Line> listPerpendicularsF = tool.GetPerpendiculars(line, listPointsOnWall);
            foreach (var perpendicular in listPerpendicularsF)
            {
                canvas.Children.Add(tool.BuildInstallAxis(BoundingBoxLines, perpendicular));
            }

            gridPoints.Clear();
            gridPoints = tool.GetGridPoints(listPerpendicularsF, wallNormals);

            ElementPreview elPreview = new ElementPreview();
            Properties.Settings.Default.InstallPoints = string.Empty;
            foreach (var item in elPreview.AddElementsPreviewF(this))
            {
                double revitX = (item.X - Derrivation.X) * Scale / (25.4 * 12);
                double revitY = -(item.Y - Derrivation.Y) * Scale / (25.4 * 12);
                Properties.Settings.Default.InstallPoints += revitX + "*" + revitY + "\n";
            }
            textBoxQuantity.Text = "Items: " + CountInstallElements();

            Dimension dimension = new Dimension();
            dimension.DrawWallDimension(line, this);
            foreach (Line item in WallPartsAfterSplit(listPointsOnWall, line))
            {
                Dimension partDim = new Dimension(30, 7, HorizontalAlignment.Center);
                partDim.DrawWallDimension(item, this);
            }
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

        private List<PointF> GetListPointsOnWall(Line line)
        {
            List<PointF> listPointsOnWall;
            WpfCoordinates wpfCoord = new WpfCoordinates();

            if (radioEqual.IsChecked == true)
            {
                int parts = Convert.ToInt32(this.TextBoxSplitPartNumber.Text);
                listPointsOnWall = wpfCoord.SplitLineEqual(line, parts);
            }
            else if (radioProportoinal.IsChecked == true)
            {
                int parts = Convert.ToInt32(TextBoxSplitPartNumber.Text);
                listPointsOnWall = wpfCoord.SplitLineProportional(line, parts);
            }
            else
            {
                double distance = Convert.ToDouble(TextBoxDistance.Text) / Scale;
                listPointsOnWall = wpfCoord.SplitLineDistance(line, distance);
            }
            return listPointsOnWall;
        }

        private void TextBoxDistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (radioDistance.IsChecked == false)
                radioDistance.IsChecked = true;
        }

        private List<Line> WallPartsAfterSplit(List<PointF> points, Line wall)
        {
            List<Line> parts = new List<Line>();

            Line startA = new Line();
            startA.X1 = wall.X1;
            startA.Y1 = wall.Y1;
            startA.X2 = points[0].X;
            startA.Y2 = points[0].Y;

            Line startB = new Line();
            startB.X1 = points[0].X;
            startB.Y1 = points[0].Y;
            startB.X2 = wall.X2;
            startB.Y2 = wall.Y2;

            Line endA = new Line();
            endA.X1 = points[points.Count - 1].X;
            endA.Y1 = points[points.Count - 1].Y;
            endA.X2 = wall.X2;
            endA.Y2 = wall.Y2;

            Line endB = new Line();
            endB.X1 = points[points.Count - 1].X;
            endB.Y1 = points[points.Count - 1].Y;
            endB.X2 = wall.X1;
            endB.Y2 = wall.Y1;

            Line startline = new Line();
            Line endLine = new Line();

            if (tool.GetLength(startA) < tool.GetLength(startB))
            {
                startline = startA;
                endLine = endA;
            }
            else
            {
                startline = startB;
                endLine = endB;
            }

            parts.Add(startline);
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
    }
}
