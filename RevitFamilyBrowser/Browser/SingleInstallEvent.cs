using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitFamilyBrowser.Revit_Classes
{
    public class SingleInstallEvent : IExternalEventHandler
    {
        public void Execute(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;           

            string FamilyPath = Properties.Settings.Default.FamilyPath;
            string FamilySymbol = Properties.Settings.Default.FamilySymbol;
            string FamilyName = Properties.Settings.Default.FamilyName;

            if (string.IsNullOrEmpty(FamilyPath))
            {
                FamilySymbol historySymbol = null;
               // TaskDialog.Show("History Ok", FamilySymbol);
                Family historyFamily = new FilteredElementCollector(doc).OfClass(typeof(Family))
                        .FirstOrDefault(e => e.Name.Equals(FamilyName)) as Family;

                ISet<ElementId> HISTORYfamilySymbolId = historyFamily.GetFamilySymbolIds();
                foreach (ElementId id in HISTORYfamilySymbolId)
                {
                    // Get name from buffer to compare
                    if (historyFamily.Document.GetElement(id).Name == FamilySymbol && FamilySymbol != null)
                        historySymbol = historyFamily.Document.GetElement(id) as FamilySymbol;
                }               
                uidoc.PostRequestForElementTypePlacement(historySymbol);              
            }

            else
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfCategory(BuiltInCategory.OST_ElectricalFixtures);
                collector.OfClass(typeof(Family));

                FamilySymbol symbol = collector.FirstElement() as FamilySymbol;
                Family family = FindFamilyByName(doc, typeof(Family), FamilyPath) as Family;

                if (null == family)
                {
                    using (var trans = new Transaction(doc, "Insert Transaction"))
                    {
                        //Family family = new Family();
                        trans.Start();
                        if (!doc.LoadFamily(FamilyPath, out family))
                        {
                            TaskDialog.Show("Loading", "Unable to load " + FamilyPath);
                        }
                        trans.Commit();
                    }
                }

                ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();
                foreach (ElementId id in familySymbolId)
                {
                    // Get name from buffer to compare
                    if (family.Document.GetElement(id).Name == FamilySymbol && FamilySymbol != null)
                        symbol = family.Document.GetElement(id) as FamilySymbol;
                }
                uidoc.PostRequestForElementTypePlacement(symbol);
            }
           
        }

        private Element FindFamilyByName(Document doc, Type targetType, string familyPath)
        {
            if (familyPath != null)
            {
                int indexSlash = familyPath.LastIndexOf("\\") + 1;
                string FamilyName = familyPath.Substring(indexSlash);
                string targetName = FamilyName.Substring(0, FamilyName.Length - 4);
                return
                    new FilteredElementCollector(doc).OfClass(targetType)
                        .FirstOrDefault(e => e.Name.Equals(targetName));
            }
            TaskDialog.Show("FamilyPath Error", "Directory can't be found ");
            return null;
        }

        public string GetName()
        {
            return "External Event";
        }
    }
}
