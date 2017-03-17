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
using System.Reflection;
using System.Drawing;

namespace RevitFamilyBrowser.WPF_Classes
{
    public partial class DockPanel : UserControl, IDockablePaneProvider
    {
        private ExternalEvent m_ExEvent;
        private MyEvent m_Handler;

        private string temp = string.Empty;
        private string collectedData = string.Empty;
        private int ImageListLength = 0;

        public DockPanel(ExternalEvent exEvent, MyEvent handler)
        {
            InitializeComponent();
            m_ExEvent = exEvent;
            m_Handler = handler;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();

            CreateEmptyFamilyImage();
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
        public void GenerateHistoryGrid()
        {
            string[] ImageList = Directory.GetFiles(System.IO.Path.GetTempPath() + "FamilyBrowser\\");
           
            if (collectedData != Properties.Settings.Default.CollectedData || ImageList.Length != ImageListLength)
            {
               // MessageBox.Show("History Updated");
                ImageListLength = ImageList.Length;
                collectedData = Properties.Settings.Default.CollectedData;
                ObservableCollection<FamilyData> collectionData = new ObservableCollection<FamilyData>();
                List<string> listData = new List<string>(collectedData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
                DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetTempPath() + "FamilyBrowser\\RevitLogo.png");
                foreach (var item in listData)
                {
                    int index = item.IndexOf('#');
                    string[] symbols = item.Substring(index + 1).Split('#');
                    foreach (var symbol in symbols)
                    {
                        FamilyData projectInstance = new FamilyData();
                        projectInstance.Name = symbol;
                       // DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetTempPath() + "FamilyBrowser\\RevitLogo.png");
                        projectInstance.img = new Uri(di.ToString());

                        try
                        {
                            projectInstance.FamilyName = item.Substring(0, index);
                        }
                        catch (Exception)
                        {
                            projectInstance.FamilyName = "NO FAMILY NAME";
                        }

                        foreach (var imageName in ImageList)
                        {
                            if (imageName.Contains(projectInstance.Name))
                            {
                                projectInstance.img = new Uri(imageName);
                            }
                        }
                        collectionData.Add(projectInstance);
                    }
                }

                foreach (var symbol in collectionData)
                {
                    if (symbol.img == new Uri(di.ToString()))                      
                        foreach (var item in collectionData)
                        {
                            if (item.FamilyName == symbol.FamilyName && item.img != new Uri(di.ToString()))                              
                                symbol.img = item.img;
                        }
                }

                ListCollectionView collectionProject = new ListCollectionView(collectionData);
                collectionProject.GroupDescriptions.Add(new PropertyGroupDescription("FamilyName"));
                dataGridHistory.ItemsSource = collectionProject;
            }
        }        

        public void GenerateGrid()
        {
            string[] ImageList = Directory.GetFiles(System.IO.Path.GetTempPath() + "FamilyBrowser\\");

            if (temp != Properties.Settings.Default.SymbolList)
            {
               // MessageBox.Show("History Upgraded");
                temp = Properties.Settings.Default.SymbolList;
                string category = Properties.Settings.Default.RootFolder;
                label_CategoryName.Content = " " + category.Substring(category.LastIndexOf("\\") + 1);

                List<string> list = new List<string>(temp.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
                ObservableCollection<FamilyData> fi = new ObservableCollection<FamilyData>();

                foreach (var item in list)
                {
                    FamilyData instance = new FamilyData();
                    int index = item.IndexOf(' ');
                    instance.Name = item.Substring(0, index);
                    instance.FullName = item.Substring(index + 1);

                    string Name = item.Substring(index + 1);
                    Name = Name.Substring(Name.LastIndexOf("\\") + 1);
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
            GenerateHistoryGrid();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }     
        private void CreateEmptyFamilyImage()
        {
            string TempImgFolder = System.IO.Path.GetTempPath() + "FamilyBrowser\\";
            if (!System.IO.Directory.Exists(TempImgFolder))
            {
                System.IO.Directory.CreateDirectory(TempImgFolder);
            }
            ImageConverter converter = new ImageConverter();
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetTempPath() + "FamilyBrowser\\RevitLogo.png");
            File.WriteAllBytes(di.ToString(), (byte[])converter.ConvertTo(Properties.Resources.RevitLogo, typeof(byte[])));
        }

        private void Expander_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            m_ExEvent.Raise();
            var instance = dataGridHistory.SelectedItem as FamilyData;          
            Properties.Settings.Default.FamilyPath = string.Empty;
            Properties.Settings.Default.FamilySymbol = instance.Name;
            Properties.Settings.Default.FamilyName = instance.FamilyName;
        }
    }
}
