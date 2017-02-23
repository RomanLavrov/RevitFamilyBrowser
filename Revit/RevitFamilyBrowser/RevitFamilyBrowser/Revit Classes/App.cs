#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using RevitFamilyBrowser.WPF_Classes;
#endregion

namespace RevitFamilyBrowser
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            a.CreateRibbonTab("Families Browser");
            RibbonPanel G17 = a.CreateRibbonPanel("Families Browser", "Families Browser");
            string path = Assembly.GetExecutingAssembly().Location;
            DockPanel dockPanel = new DockPanel();
            DockablePaneId dpID = new DockablePaneId(new Guid("FA0C04E6-F9E7-413A-9D33-CFE32622E7B8"));
            a.RegisterDockablePane(dpID, "FamilyBrowser", (IDockablePaneProvider)dockPanel);

            PushButtonData btnShow = new PushButtonData("ShowPanel", "Show\nPanel", path, "RevitFamilyBrowser.Revit_Classes.ShowPanel");
            PushButtonData btnFolder = new PushButtonData("OpenFolder", "Open\nFolder", path, "RevitFamilyBrowser.Revit_Classes.FolderSelect");
            

            RibbonItem ri1 = G17.AddItem(btnShow);
            RibbonItem ri2 = G17.AddItem(btnFolder);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
