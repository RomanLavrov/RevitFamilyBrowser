#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows;
using RevitFamilyBrowser.WPF_Classes;
#endregion

namespace RevitFamilyBrowser
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {      
      //  DockPanel browser = new DockPanel();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

           // DockablePaneProviderData dpData = new DockablePaneProviderData();
           // DockPanel browser = new DockPanel();
          //  dpData.FrameworkElement = browser as System.Windows.FrameworkElement;

            //List<RibbonPanel> ribbon = uiapp.GetRibbonPanels();

            //foreach (var item in ribbon)
            //{
            //    TaskDialog.Show("ribbon", item.ToString());
            //}

            //FilteredElementCollector col
            //  = new FilteredElementCollector(doc)
            //    .WhereElementIsNotElementType()
            //    .OfCategory(BuiltInCategory.INVALID)
            //    .OfClass(typeof(Wall));        

            return Result.Succeeded;
        }
    }
}
