using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.IO;
using RevitFamilyBrowser.WPF_Classes;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class FolderSelect : IExternalCommand
    {
        public List<string> FamilyPath { get; set; }
        public List<string> FamilyName { get; set; }       
        public List<string> SymbolName { get; set; }     
                
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            DockPanel panel = new DockPanel();
          
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = Properties.Settings.Default.RootFolder;
            List<string> Directories = new List<string>();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.RootFolder = fbd.SelectedPath;
                Properties.Settings.Default.Save();
                Directories = Directory.GetDirectories(fbd.SelectedPath).ToList();
            }
            else
            {                
                return Result.Cancelled;
            }                

            FamilyPath = GetFamilyPath(fbd.SelectedPath);
            FamilyName = GetFamilyName(FamilyPath);          
            Properties.Settings.Default.SymbolList = string.Empty;

            foreach (var item in GetSymbols(FamilyPath, doc))
            {
                Properties.Settings.Default.SymbolList += item + "\n";
            }
           
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

        public List<string> GetFamilyPath(string dir)
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
                //TaskDialog.Show("Families not found", "Try to select other folder");
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

        public List<string> GetSymbols(List<string> FamilyPath, Document doc)
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
                        FamilyInstance.Add(symbol.Name.ToString() + " " + item);
                    }
                }
                transaction.RollBack();
                return FamilyInstance;
            }
        }       
    }
}
