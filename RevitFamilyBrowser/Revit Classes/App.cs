#region Namespaces
using System;
using Autodesk.Revit.UI;
using System.Reflection;
using RevitFamilyBrowser.WPF_Classes;
using RevitFamilyBrowser.Revit_Classes;
using RevitFamilyBrowser.Properties;
using System.IO;

#endregion

namespace RevitFamilyBrowser
{
    class App : IExternalApplication
    {
        public static App thisApp = null;
        public Result OnStartup(UIControlledApplication a)
        {
            a.CreateRibbonTab("Familien Browser"); //Familien Browser Families Browser
            RibbonPanel G17 = a.CreateRibbonPanel("Familien Browser", "Familien Browser"); 
            string path = Assembly.GetExecutingAssembly().Location;

            MyEvent handler = new MyEvent();
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            DockPanel dockPanel = new DockPanel(exEvent, handler);
            DockablePaneId dpID = new DockablePaneId(new Guid("FA0C04E6-F9E7-413A-9D33-CFE32622E7B8"));
            a.RegisterDockablePane(dpID, "Familien Browser", (IDockablePaneProvider)dockPanel);

            PushButtonData btnShow = new PushButtonData("ShowPanel", "Panel\nanzeigen", path, "RevitFamilyBrowser.Revit_Classes.ShowPanel"); //Panel anzeigen ShowPanel
            btnShow.LargeImage = GetImage(Resources.IconShowPanel.GetHbitmap());
            RibbonItem ri1 = G17.AddItem(btnShow);

            PushButtonData btnFolder = new PushButtonData("OpenFolder", "Verzeichnis\nöffnen", path, "RevitFamilyBrowser.Revit_Classes.FolderSelect");   //Verzeichnis  öffnen        
            btnFolder.LargeImage = GetImage(Resources.OpenFolder.GetHbitmap());            
            RibbonItem ri2 = G17.AddItem(btnFolder);

            PushButtonData btnImage = new PushButtonData("Image", "Image", path, "RevitFamilyBrowser.Revit_Classes.Image");
            RibbonItem ri3 = G17.AddItem(btnImage);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetTempPath() + "FamilyBrowser\\");
            foreach (var imgfile in di.GetFiles())
            {
                if (imgfile.LastAccessTime > new DateTime (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1))
                {
                    try
                    {
                        imgfile.Delete();
                    }
                    catch (Exception)
                    {

                    }
                }
                         
            }
            return Result.Succeeded;           
        }

        private System.Windows.Media.Imaging.BitmapSource GetImage(IntPtr bm)
        {
            System.Windows.Media.Imaging.BitmapSource bmSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bm,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            return bmSource;
        }
    }
}
