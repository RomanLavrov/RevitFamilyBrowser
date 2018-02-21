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
using ArgumentException = Autodesk.Revit.Exceptions.ArgumentException;

namespace zRevitFamilyBrowser.Revit_Classes
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

            double Offset = Properties.Settings.Default.Offset/(25.4*12);
           
            string familyPath = Properties.Settings.Default.FamilyPath;
            string familySymbol = Properties.Settings.Default.FamilySymbol;
            string familyName = Properties.Settings.Default.FamilyName;
            var insertionPoints = GetInsertionPoints();
            FamilySymbol targetSymbol;

            if (string.IsNullOrEmpty(familyPath))
            {
                targetSymbol = GetSymbolHistory(doc, familyName, familySymbol);
            }
            else
            {
                targetSymbol = GetSymbolNew(doc, familyPath, familySymbol);
            }

            foreach (var item in insertionPoints)
            {
                using (var transact = new Transaction(doc, "Insert Symbol"))
                {
                    transact.Start();
                    XYZ point = new XYZ(item.X, item.Y , Offset);
                    Level level = view.GenLevel;
                    Element host = level as Element;
                    targetSymbol.Activate();
                    doc.Create.NewFamilyInstance(point, targetSymbol, host, StructuralType.NonStructural);
                    transact.Commit();
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
                return new FilteredElementCollector(doc).OfClass(targetType)
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

        private FamilySymbol GetSymbolNew(Document doc, string familyPath, string familySymbol)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_ElectricalFixtures);
            collector.OfClass(typeof(Family));

            FamilySymbol symbol = collector.FirstElement() as FamilySymbol;
            Family family = FindFamilyByName(doc, typeof(Family), familyPath) as Family;

            if (null == family)
            {
                using (var trans = new Transaction(doc, "Load Family"))
                {
                    trans.Start();
                    doc.LoadFamily(familyPath, out family);
                    trans.Commit();
                }
            }

            ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();
            foreach (ElementId id in familySymbolId)
            {
                if (family.Document.GetElement(id).Name == familySymbol && familySymbol != null)
                    symbol = family.Document.GetElement(id) as FamilySymbol;
            }

            return symbol;
        }

        private FamilySymbol GetSymbolHistory(Document doc, string FamilyName, string familySymbol)
        {
            FamilySymbol historySymbol = null;
            Family historyFamily = new FilteredElementCollector(doc).OfClass(typeof(Family)).FirstOrDefault(e => e.Name.Equals(FamilyName)) as Family;
            ISet<ElementId> historyFamilySymbolId = historyFamily.GetFamilySymbolIds();
            foreach (ElementId id in historyFamilySymbolId)
            {
                if (historyFamily.Document.GetElement(id).Name == familySymbol && familySymbol != null)
                    historySymbol = historyFamily.Document.GetElement(id) as FamilySymbol;
            }

            return historySymbol;
        }

    }
}
