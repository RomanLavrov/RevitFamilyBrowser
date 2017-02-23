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
using Autodesk.Revit.UI;

namespace RevitFamilyBrowser.WPF_Classes
{
    /// <summary>
    /// Interaction logic for Pane.xaml
    /// </summary>
    public partial class DockPanel : UserControl, IDockablePaneProvider
    {       
        public DockPanel()
        {
            InitializeComponent();           
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState.DockPosition = DockPosition.Left;
        }       

        public static implicit operator Window(DockPanel v)
        {
            throw new NotImplementedException();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Command command = new Command();
          //  command.Execute(, );
        }
    }
}
