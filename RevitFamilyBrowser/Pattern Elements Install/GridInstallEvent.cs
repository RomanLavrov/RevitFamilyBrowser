using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class GridInstallEvent : IExternalEventHandler
    {
        public void Execute(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;

            string FamilyPath = Properties.Settings.Default.FamilyPath;
            string FamilySymbol = Properties.Settings.Default.FamilySymbol;
            string FamilyName = Properties.Settings.Default.FamilyName;
            var insertionPoints = GetInsertionPoints();

            if (string.IsNullOrEmpty(FamilyPath))
            {
                //MessageBox.Show("Elemnt from history");
                FamilySymbol historySymbol = null;
                Family historyFamily = new FilteredElementCollector(doc).OfClass(typeof(Family)).FirstOrDefault(e=>e.Name.Equals(FamilyName)) as Family;
                ISet<ElementId> historyFamilySymbolId = historyFamily.GetFamilySymbolIds();
                foreach (ElementId id in historyFamilySymbolId)
                {
                    if (historyFamily.Document.GetElement(id).Name == FamilySymbol && FamilySymbol != null)
                        historySymbol = historyFamily.Document.GetElement(id) as FamilySymbol;
                }

                foreach (var item in insertionPoints)
                {
                    using (var transact = new Transaction(doc, "Insert Symbol"))
                    {
                        transact.Start();
                        XYZ point = new XYZ(item.X, item.Y, 0);
                        Level level = view.GenLevel;
                        Element host = level as Element;
                        doc.Create.NewFamilyInstance(point, historySymbol, host, StructuralType.NonStructural);
                        transact.Commit();
                    }
                }
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
                    using (var trans = new Transaction(doc, "Load Family"))
                    {
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

                foreach (var item in insertionPoints)
                {
                    using (var transact = new Transaction(doc, "Insert Symbol"))
                    {
                        transact.Start();
                        XYZ point = new XYZ(item.X, item.Y, 0);
                        Level level = view.GenLevel;
                        Element host = level as Element;
                        doc.Create.NewFamilyInstance(point, symbol, host, StructuralType.NonStructural);
                        transact.Commit();
                    }
                }
            }
        }

        public string GetName()
        {
            return "GridInstall Event";
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

        private List<XYZ> GetInsertionPoints()
        {
            var insertionPoints = new List<XYZ>();
            List<string> buffer = Properties.Settings.Default.InstallPoints.Split(new[] { '\r', '\n' }).ToList();
            int count = 0;
            buffer = buffer.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            foreach (var item in buffer)
            {
                int separator = item.IndexOf('*');
                XYZ installPoint = new XYZ(Convert.ToDouble(item.Substring(0, separator)), Convert.ToDouble(item.Substring(separator + 1)), 0);
                count++;
                Properties.Settings.Default.InstallPoints = string.Empty;
                insertionPoints.Add(installPoint);
            }
            return insertionPoints;
        }
    }
}
