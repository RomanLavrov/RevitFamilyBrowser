using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.UI;
using RevitFamilyBrowser.Revit_Classes;
using System.IO;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace RevitFamilyBrowser.WPF_Classes
{    
    public partial class DockPanel : UserControl, IDockablePaneProvider
    {
        private ExternalEvent m_ExEvent;
        private MyEvent m_Handler;
        private string temp = string.Empty;

        public DockPanel(ExternalEvent exEvent, MyEvent handler)
        {
            InitializeComponent();

            m_ExEvent = exEvent;
            m_Handler = handler;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();
        }

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

        public void GenerateGrid()
        {
            if (temp != Properties.Settings.Default.SymbolList)
            {
                temp = Properties.Settings.Default.SymbolList;

                string[] ImageList = Directory.GetFiles(System.IO.Path.GetTempPath() + "FamilyBrowser\\");
                string category = Properties.Settings.Default.RootFolder;
                label_CategoryName.Content =" " + category.Substring(category.LastIndexOf("\\")+1);                

                List<string> list = new List<string>(temp.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
                ObservableCollection<FamilyData> fi = new ObservableCollection<FamilyData>();

                foreach (var item in list)
                {
                    FamilyData instance = new FamilyData();
                    int index = item.IndexOf(' ');
                    instance.Name = item.Substring(0, index);
                    instance.FullName = item.Substring(index + 1);

                    string Name = item.Substring(index + 1);
                    Name = Name.Substring(Name.LastIndexOf("\\")+1);
                    Name = Name.Substring(0, Name.IndexOf('.'));
                    instance.FamilyName = Name;

                    foreach (var imageName in ImageList)
                    {
                        if (imageName.Contains(instance.Name.TrimEnd()))
                        {
                            instance.img = new Uri(imageName);
                        }
                        
                    }                                   
                    fi.Add(instance);                                       
                }
                //------Collection to sort data in XAML------
                ListCollectionView collection = new ListCollectionView(fi);
                collection.GroupDescriptions.Add(new PropertyGroupDescription("FamilyName"));
                dataGrid.ItemsSource = collection;
            }
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            m_ExEvent.Raise();
            var instance = dataGrid.SelectedItem as FamilyData;
            Properties.Settings.Default.FamilyPath = instance.FullName;
            Properties.Settings.Default.FamilySymbol = instance.Name;
        }

        private void drag_DragEnter(object sender, DragEventArgs e)
        {
            m_ExEvent.Raise();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            GenerateGrid();           
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }   
}
