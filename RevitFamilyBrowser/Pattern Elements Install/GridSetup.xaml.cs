using Autodesk.Revit.UI;
using RevitFamilyBrowser.Revit_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevitFamilyBrowser.WPF_Classes
{
    /// <summary>
    /// Interaction logic for Grid.xaml
    /// </summary>
    public partial class GridSetup : UserControl
    {
        private ExternalEvent m_ExEvent;
        private GridInstallEvent m_Handler;

        public GridSetup(ExternalEvent exEvent, GridInstallEvent handler)
        {
            InitializeComponent();
            radioEqual.IsChecked = true;
            m_ExEvent = exEvent;
            m_Handler = handler;
            TextBoxSymbol.Text = Properties.Settings.Default.FamilySymbol;
            TextBoxFamily.Text = Properties.Settings.Default.FamilyName;
            ImageSymbol.Source = new BitmapImage(new Uri(GetImage()));
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

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            //Properties.Settings.Default.InstallPoints = string.Empty;
        }

        private void ButtonInsertClick(object sender, RoutedEventArgs e)
        {
            m_ExEvent.Raise();
            this.TextBoxSymbol.Text = string.Empty;
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null) parentWindow.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null) parentWindow.Close();
        }

        private string GetImage()
        {
            MessageBox.Show(Properties.Settings.Default.FamilyName);
            string[] ImageList = Directory.GetFiles(System.IO.Path.GetTempPath() + "FamilyBrowser\\");
            MessageBox.Show(ImageList.Length.ToString());
            string imageUri = imageUri = (System.IO.Path.GetTempPath() + "FamilyBrowser\\RevitLogo.png").ToString();
            foreach (var imageName in ImageList)
            {
                if (imageName.Contains(Properties.Settings.Default.FamilySymbol))
                {
                    MessageBox.Show("Found");
                    imageUri = imageName;
                }
            }
            MessageBox.Show(imageUri);
            return imageUri;
        }
    }
}
