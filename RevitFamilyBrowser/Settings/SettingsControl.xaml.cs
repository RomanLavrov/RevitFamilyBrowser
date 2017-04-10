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
using Ookii.Dialogs.Wpf;
using System.IO;

namespace RevitFamilyBrowser.Settings
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private string DefaultFamilyPath { get; set; }
        public SettingsControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog ofd = new VistaFolderBrowserDialog();
            if (ofd.ShowDialog() == true)
            {
                TextBoxFamily.Text = ofd.SelectedPath;
                DefaultFamilyPath = ofd.SelectedPath;
            }
        }

        private void buttonSaveIni_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog ofd = new VistaFolderBrowserDialog();
            if (ofd.ShowDialog() == true)
            {
                string path = ofd.SelectedPath;
                string fileName = "FamilyBrowser.ini";
                TextBoxSettings.Text = ofd.SelectedPath + fileName;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            string path = TextBoxSettings.Text;
            string fileName = "FamilyBrowser.ini";
            File.WriteAllText(path + fileName, DefaultFamilyPath+@"\");
            Properties.Settings.Default.SettingPath = path + fileName;
            Properties.Settings.Default.Save();

            var parentWindow = Window.GetWindow(this);
            parentWindow.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            parentWindow.Close();
        }
    }
}
