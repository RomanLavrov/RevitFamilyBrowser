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
    public partial class SettingsControl : UserControl
    {
        private string DefaultFamilyPath { get; set; }
        public SettingsControl()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DefaultSettingsPath))
                this.TextBoxFamily.Text = Properties.Settings.Default.DefaultFamilyPth;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.DefaultSettingsPath))
                this.TextBoxSettings.Text = Properties.Settings.Default.DefaultSettingsPath;

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
                string fileName = @"\FamilyBrowser.ini";
                TextBoxSettings.Text = ofd.SelectedPath + fileName;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            string pathIni = TextBoxSettings.Text;
          //  string fileName = "FamilyBrowser.ini";
            var parentWindow = Window.GetWindow(this);
            bool valid = true;

            try
            {
                System.IO.Path.GetFullPath(pathIni);
            }
            catch (Exception)
            {
                valid = false;
                parentWindow.Close();
            }

            try
            {
                System.IO.Path.GetFullPath(DefaultFamilyPath);
            }
            catch (Exception)
            {
                DefaultFamilyPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (valid)
            {
                File.WriteAllText(pathIni, DefaultFamilyPath + @"\");
                Properties.Settings.Default.SettingPath = pathIni;
                Properties.Settings.Default.Save();
            }

            Properties.Settings.Default.DefaultFamilyPth = DefaultFamilyPath;
            Properties.Settings.Default.DefaultSettingsPath = pathIni;

            parentWindow?.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}
