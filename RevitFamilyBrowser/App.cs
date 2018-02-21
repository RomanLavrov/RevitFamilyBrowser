#region Namespaces
using System;
using Autodesk.Revit.UI;
using System.Reflection;
using zRevitFamilyBrowser.WPF_Classes;
using zRevitFamilyBrowser.Revit_Classes;
using zRevitFamilyBrowser.Properties;
using System.IO;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.Generic;
using Autodesk.Revit.UI.Events;
using System.Linq;

#endregion

namespace zRevitFamilyBrowser
{
    class App : IExternalApplication
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public Result OnStartup(UIControlledApplication a)
        {
            a.CreateRibbonTab("Familien Browser"); //Familien Browser Families Browser
            RibbonPanel G17 = a.CreateRibbonPanel("Familien Browser", "Familien Browser");
            string path = Assembly.GetExecutingAssembly().Location;

            SingleInstallEvent handler = new SingleInstallEvent();
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            DockPanel dockPanel = new DockPanel(exEvent, handler);
            DockablePaneId dpID = new DockablePaneId(new Guid("FA0C04E6-F9E7-413A-9D33-CFE32622E7B8"));
            a.RegisterDockablePane(dpID, "Familien Browser", (IDockablePaneProvider)dockPanel);

            PushButtonData btnShow = new PushButtonData("ShowPanel", "Panel\nanzeigen", path, "zRevitFamilyBrowser.Revit_Classes.ShowPanel"); //Panel anzeigen ShowPanel
            btnShow.LargeImage = Tools.GetImage(Resources.IconShowPanel.GetHbitmap());
            RibbonItem ri1 = G17.AddItem(btnShow);

            PushButtonData btnFolder = new PushButtonData("OpenFolder", "Verzeichnis\nöffnen", path, "zRevitFamilyBrowser.Revit_Classes.FolderSelect");   //Verzeichnis  öffnen      
            btnFolder.LargeImage = Tools.GetImage(Resources.OpenFolder.GetHbitmap());
            RibbonItem ri2 = G17.AddItem(btnFolder);

            PushButtonData btnSpace = new PushButtonData("Space", "Grid Elements\nInstall", path, "zRevitFamilyBrowser.Revit_Classes.Space");
            btnSpace.LargeImage = Tools.GetImage(Resources.Grid.GetHbitmap());
            btnSpace.ToolTip =
                "1. Select item form browser.\n2. Pick room in project\n3. Adjust item position and quantity.";
            RibbonItem ri3 = G17.AddItem(btnSpace);

            G17.AddSeparator();
            PushButtonData btnSettings = new PushButtonData("Settings", "Settings", path, "zRevitFamilyBrowser.Revit_Classes.Settings");
            btnSettings.LargeImage = Tools.GetImage(Resources.settings.GetHbitmap());
            RibbonItem ri4 = G17.AddItem(btnSettings);

            // a.ControlledApplication.DocumentChanged += OnDocChanged;
            a.ControlledApplication.DocumentOpened += OnDocOpened;
            a.ViewActivated += OnViewActivated;

            if (File.Exists(Properties.Settings.Default.SettingPath))
            {
                Properties.Settings.Default.RootFolder = File.ReadAllText(Properties.Settings.Default.SettingPath);
                Properties.Settings.Default.Save();
            }

            return Result.Succeeded;
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            a.ControlledApplication.DocumentOpened -= OnDocOpened;
           // a.ControlledApplication.DocumentChanged -= OnDocChanged;
            a.ViewActivated -= OnViewActivated;


            Properties.Settings.Default.FamilyPath = string.Empty;
            Properties.Settings.Default.FamilyName = string.Empty;
            Properties.Settings.Default.FamilySymbol = string.Empty;
            Properties.Settings.Default.Save();
          
            return Result.Succeeded;
        }

        private void OnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            Tools.CreateImages(e.Document);
            Tools.CollectFamilyData(e.Document);
        }

        private void OnDocOpened(object sender, DocumentOpenedEventArgs e)
        {
            Tools.CreateImages(e.Document);
            Tools.CollectFamilyData(e.Document);
        }

        //private void OnDocChanged(object sender, DocumentChangedEventArgs e)
        //{
        //    Tools.CreateImages(e.GetDocument());
        //    Tools.CollectFamilyData(e.GetDocument());
        //}

        private void OnDocSaved(object sender, DocumentSavedEventArgs e)
        {
            //Tools.CreateImages(e.Document);
            //Tools.CollectFamilyData(e.Document);
        }
    }
}