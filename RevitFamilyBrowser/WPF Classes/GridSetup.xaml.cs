using System;
using System.Collections.Generic;
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
        public GridSetup()
        {
            InitializeComponent();
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

        private void buttonAddVertical_Click(object sender, RoutedEventArgs e)
        {
            int temp = int.Parse(textBoxVertical.Text);
            temp++;
            textBoxVertical.Text = temp.ToString();
        }

        private void buttonRemoveVertical_Click(object sender, RoutedEventArgs e)
        {
            int temp = int.Parse(textBoxVertical.Text);
            if (temp > 0)
                temp--;
            textBoxVertical.Text = temp.ToString();
        }
    }
}
