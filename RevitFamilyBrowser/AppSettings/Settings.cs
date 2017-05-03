using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows;
using RevitFamilyBrowser.Settings;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    class Settings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SettingsControl settings = new SettingsControl();

            Window window = new Window
            {
                Height = 210,
                Width = 600,
                Title = "Familien Browser Settings",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = settings,
                Background = System.Windows.Media.Brushes.WhiteSmoke,
                WindowStyle = WindowStyle.ToolWindow,
                Name = "Settings",
                ResizeMode = ResizeMode.NoResize
            };
            window.Show();
            return Result.Succeeded;
        }
    }
}
