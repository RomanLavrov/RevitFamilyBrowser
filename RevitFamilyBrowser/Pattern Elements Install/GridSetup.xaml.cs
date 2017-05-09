using Autodesk.Revit.UI;
using RevitFamilyBrowser.Pattern_Elements_Install;
using RevitFamilyBrowser.Revit_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        public GridSetup(ExternalEvent exEvent, GridInstallEvent handler)
        {
            InitializeComponent();
            m_ExEvent = exEvent;
            m_Handler = handler;

            radioEqual.IsChecked = true;

            TextBoxSymbol.Text = " Type: " + Properties.Settings.Default.FamilySymbol;
            TextBoxFamily.Text = " Family: " + Properties.Settings.Default.FamilyName;
            ImageSymbol.Source = new BitmapImage(new Uri(GetImage()));
            comboBoxHeight.ItemsSource = Enum.GetValues(typeof(Heights));
        }

        private void buttonAddHorizontal_Click(object sender, RoutedEventArgs e)
        {
            int temp = int.Parse(textBoxHorizontal.Text);
            temp++;
            textBoxHorizontal.Text = temp.ToString();
        }

        private void buttonRemoveHorizontal_Click(object sender, RoutedEventArgs e)
        {
            int temp = int.Parse(textBoxHorizontal.Text);
            if (temp > 0)
                temp--;
            textBoxHorizontal.Text = temp.ToString();
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

        public void ButtonInsertClick(object sender, RoutedEventArgs e)
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

            Properties.Settings.Default.FamilyPath = string.Empty;
            Properties.Settings.Default.FamilyName = string.Empty;
            Properties.Settings.Default.FamilySymbol = string.Empty;
            Properties.Settings.Default.Save();
        }

        private string GetImage()
        {
            string[] ImageList = Directory.GetFiles(System.IO.Path.GetTempPath() + "FamilyBrowser\\");
            string imageUri = imageUri = (System.IO.Path.GetTempPath() + "FamilyBrowser\\RevitLogo.png").ToString();
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

        public void line_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Equals(((System.Windows.Shapes.Line)sender).Stroke, Brushes.Red))
            {
                ((System.Windows.Shapes.Line)sender).Stroke = Brushes.Black;
            }
        }

        public void line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Change line colour back to normal 
            ((System.Windows.Shapes.Line)sender).Stroke = System.Windows.Media.Brushes.Red;
        }

        public void line_MouseEnter(object sender, MouseEventArgs e)
        {
            ((System.Windows.Shapes.Line)sender).Stroke = Brushes.Gray;
        }

        public List<Line> GetWpfWalls(List<Line> revitWalls, int derrivationX, int derrivationY, int Scale)
        {
            List<Line> wpfWalls = new List<Line>();
            foreach (var item in revitWalls)
            {
                System.Windows.Shapes.Line myLine = new System.Windows.Shapes.Line();
                myLine.X1 = (item.X1 / Scale) + derrivationX;
                myLine.Y1 = ((-item.Y1 / Scale) + derrivationY);
                myLine.X2 = (item.X2 / Scale) + derrivationX;
                myLine.Y2 = ((-item.Y2 / Scale) + derrivationY);
                wpfWalls.Add(myLine);
            }
            return wpfWalls;
        }

        public void GetRevitInstallCoordinates(List<Line>revitWallNormals,List<Line> revitWalls, int wallIndex, bool isEqual)
        {
            CoordinatesRevit rvt = new CoordinatesRevit();
            Line rvtWall = revitWalls[wallIndex];

            List<PointF> rvtPointsOnWall = new List<PointF>();
            if (isEqual)
            {
                rvtPointsOnWall = rvt.GetSplitPointsEqual(rvtWall, Convert.ToInt32(textBoxHorizontal.Text));
            }
            else
            {
                rvtPointsOnWall = rvt.GetSplitPointsProportional(rvtWall, Convert.ToInt32(textBoxHorizontal.Text));
            }

            List<System.Windows.Shapes.Line> rvtListPerpendiculars = rvt.GetPerpendiculars(rvtWall, rvtPointsOnWall);
            List<System.Drawing.PointF> rvtGridPoints = rvt.GetGridPointsRvt(revitWallNormals, rvtListPerpendiculars);
           

            foreach (var item in rvtGridPoints)
            {
                Properties.Settings.Default.InstallPoints += (item.X) / (25.4 * 12) + "*" + (item.Y) / (25.4 * 12) + "\n";
            }
        }

        public List<System.Drawing.Point> GetListPointsOnWall(Line line)
        {
            List<System.Drawing.Point> listPointsOnWall = new List<System.Drawing.Point>();
            WpfCoordinates wpfCoord = new WpfCoordinates();
            if (this.radioEqual.IsChecked == true)
            {
                listPointsOnWall = wpfCoord.SplitLineEqual(line, Convert.ToInt32(this.textBoxHorizontal.Text));
            }
            else
                listPointsOnWall = wpfCoord.SplitLineProportional(line, Convert.ToInt32(this.textBoxHorizontal.Text));

            return listPointsOnWall;
        }

    }
}
