//using Autodesk.Revit.UI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.ApplicationServices;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.ReadOnly)]
    class ProjectFamilyCollector : IExternalCommand
    {
        List<ElementId> _added_element_ids = new List<ElementId>();
        string temp = string.Empty;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
           
            Application myapp = app.Application;

            FilteredElementCollector families;
            families = new FilteredElementCollector(doc).OfClass(typeof(Family));           

            foreach (var item in families)
            {
                Family family = item as Family;
                FamilySymbol symbol;

                temp += item.Name;

                ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();
                foreach (ElementId id in familySymbolId)
                {
                    symbol = family.Document.GetElement(id) as FamilySymbol;
                    //  if (symbol.IsActive)
                    {
                        temp += "#" + symbol.Name;
                    }
                }
                temp += "\n";
            }           
           // Properties.Settings.Default.CollectedData = temp;
            //TaskDialog.Show("Collector", temp);
            myapp.DocumentChanged += new System.EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
            return Result.Succeeded;
        }

        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
            TaskDialog.Show("DocChanged", "Change");
            Properties.Settings.Default.CollectedData = temp;
        }
    }
}
