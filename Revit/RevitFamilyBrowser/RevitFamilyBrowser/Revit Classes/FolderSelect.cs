using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.IO;
using System.Windows;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    class FolderSelect : IExternalCommand
    {
        public List<string> FamilyPath { get; set; }
        public List<string> FamilyName { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            List<string> Directories = new List<string>();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Directories = Directory.GetDirectories(fbd.SelectedPath).ToList();
            }
            else
                return Result.Cancelled;

            FamilyPath = GetFamilyPath(fbd.SelectedPath);
            FamilyName = GetFamilyName(FamilyPath);

            DisplayList(FamilyPath);
            DisplayList(FamilyName);
            DisplayList(GetSymbols(FamilyPath, doc));
            return Result.Succeeded;
        }

        private void DisplayList(List<string> list)
        {
            string temp = string.Empty;
            foreach (var item in list)
            {
                temp += item + Environment.NewLine;
            }
            TaskDialog.Show("DisplayList", temp);
        }

        private List<string> GetFamilyPath(string dir)
        {
            List<string> FamiliesList = new List<string>();
            foreach (var item in Directory.GetFiles(dir))
            {
                if (item.Contains("rfa"))
                {
                    FamiliesList.Add(item);
                }
            }
            if (FamiliesList.Count == 0)
            {
                TaskDialog.Show("Families not found", "Try to select other folder");
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            }
            return FamiliesList;
        }

        private List<string> GetFamilyName(List<string> FamilyPath)
        {
            int index = 0;
            List<string> FamiliesName = new List<string>();
            foreach (var item in FamilyPath)
            {
                index = item.LastIndexOf('\\') + 1;
                FamiliesName.Add(item.Substring(index));
            }
            return FamiliesName;
        }

        private List<string> GetSymbols(List<string> FamilyPath, Document doc)
        {
           
            List<string> FamilyInstance = new List<string>();
            using (var transaction = new Transaction(doc, "Family Symbol Collecting"))
            {
                transaction.Start();
                foreach (var item in FamilyPath)
                {
                    Family family = null;
                    FamilySymbol symbol = null;
                    
                    if (!doc.LoadFamily(item, out family))
                    {
                       // TaskDialog.Show("Load failed", "Unable to load " + item);
                        continue;                      
                    }
                                        
                    ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();
                    foreach (ElementId id in familySymbolId)
                    {
                        symbol = family.Document.GetElement(id) as FamilySymbol;
                        FamilyInstance.Add(symbol.Name.ToString());
                    }
                }                            
                transaction.RollBack();
                return FamilyInstance;
            }
        }
    }
}
