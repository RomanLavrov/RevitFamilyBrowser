using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.IO;
using zRevitFamilyBrowser.WPF_Classes;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using Autodesk.Revit.DB.Events;
using System.Diagnostics;
using Ookii.Dialogs.Wpf;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace zRevitFamilyBrowser.Revit_Classes
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

            //TODO hotfix
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if (File.Exists(Properties.Settings.Default.DefaultSettingsPath))
            {
                if (Properties.Settings.Default.RootFolder == File.ReadAllText(Properties.Settings.Default.SettingPath))
                    fbd.SelectedPath = File.ReadAllText(Properties.Settings.Default.SettingPath);
                else
                    fbd.SelectedPath = Properties.Settings.Default.RootFolder;
            }

            else
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.RootFolder))
                    fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                else
                    fbd.SelectedPath = Properties.Settings.Default.RootFolder;
            }
           
           
            if (fbd.ShowDialog() == true)
            {
                if (fbd.SelectedPath.Contains("ROCHE") && app.VersionNumber != "2015")
                {
                    TaskDialog.Show("Warning", "Please select other family path.");
                    fbd.ShowDialog();
                }
                Properties.Settings.Default.RootFolder = fbd.SelectedPath;
                Properties.Settings.Default.Save();
            }
            else
            {
                return Result.Cancelled;
            }

            FamilyPath = GetFamilyPath(fbd.SelectedPath);
            FamilyName = GetFamilyName(FamilyPath);
            Properties.Settings.Default.SymbolList = string.Empty;
            Properties.Settings.Default.Save();

            SymbolName = GetSymbols(FamilyPath, doc);

            foreach (var item in SymbolName)
            {
                Properties.Settings.Default.SymbolList += item + "\n";
            }
            return Result.Succeeded;
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
            List<string> FamiliesName = new List<string>();
            foreach (var item in FamilyPath)
            {
                int index = item.LastIndexOf('\\') + 1;
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
                Family family = null;
                FamilySymbol symbol = null;
                foreach (var item in FamilyPath)
                {
                    if (!doc.LoadFamily(item, out family))
                    {
                        family = GetFamilyFromPath(item, doc);
                    }

                    if (family == null)
                    {
                        TaskDialog.Show("Error", item);
                        continue;
                    }
                        

                    ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();

                    foreach (ElementId id in familySymbolId)
                    {
                        symbol = family.Document.GetElement(id) as FamilySymbol;
                        if (symbol == null) continue;
                        FamilyInstance.Add(symbol.Name.ToString() + " " + item);
                       
                        string TempImgFolder = System.IO.Path.GetTempPath() + "FamilyBrowser\\";
                        string filename = TempImgFolder + symbol.Name + ".bmp";

                        if (!File.Exists(filename))
                        {
                            System.Drawing.Size imgSize = new System.Drawing.Size(200, 200);
                            Bitmap image = symbol.GetPreviewImage(imgSize);
                            BitmapEncoder encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(Tools.ConvertBitmapToBitmapSource(image)));
                            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);

                            encoder.Save(file);
                            file.Close();
                        }
                    }
                }
                transaction.RollBack();
                return FamilyInstance;
            }
        }
        
        private Family GetFamilyFromPath(string path, Document doc)
        {
            Family family = null;
            int index = path.LastIndexOf('\\') + 1;
            string familyName = path.Substring(index);
            familyName = familyName.Remove(familyName.IndexOf('.'));

            FilteredElementCollector elementCollector = new FilteredElementCollector(doc);
            elementCollector = elementCollector.OfClass(typeof(Family));
            foreach (Element element in elementCollector)
            {
                if (element.Name == familyName)
                    family = element as Family;
            }
            return family;
        }
    }
}


