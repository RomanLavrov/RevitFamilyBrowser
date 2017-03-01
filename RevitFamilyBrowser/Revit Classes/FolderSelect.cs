using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.IO;
using RevitFamilyBrowser.WPF_Classes;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using Autodesk.Revit.DB.Events;
using System.Diagnostics;

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

       //     app.DocumentChanged += new System.EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
           // CreateImages(doc);
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
            SymbolName = GetSymbols(FamilyPath, doc);

            foreach (var item in SymbolName)
            {
                Properties.Settings.Default.SymbolList += item + "\n";
            }
            return Result.Succeeded;
        }

        //private void DisplayList(List<string> list)
        //{
        //    string temp = string.Empty;
        //    foreach (var item in list)
        //    {
        //        temp += item + Environment.NewLine;
        //    }
        //    TaskDialog.Show("DisplayList", temp);
        //}

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

                        System.Drawing.Size imgSize = new System.Drawing.Size(200, 200);
                        Bitmap image = symbol.GetPreviewImage(imgSize);

                        //------Eencode image to jpeg for test display purposes:------
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(ConvertBitmapToBitmapSource(image)));
                        encoder.QualityLevel = 100;

                        //------------Create temporary folder for images--------
                        string TempImgFolder = System.IO.Path.GetTempPath() + "FamilyBrowser\\";
                        if (!System.IO.Directory.Exists(TempImgFolder))
                        {
                            System.IO.Directory.CreateDirectory(TempImgFolder);
                        }

                        string filename = TempImgFolder + symbol.Name + ".bmp";
                        //// if (!System.IO.Directory.Exists(TempImgFolder + fi.Name + ".bmp"))
                        //{
                        FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
                        encoder.Save(file);
                        file.Close();
                        //}
                        //------------------------------------------------------
                    }
                }
                transaction.RollBack();
                return FamilyInstance;
            }
        }

        static BitmapSource ConvertBitmapToBitmapSource(Bitmap bmp)
        {
            return System.Windows.Interop.Imaging
              .CreateBitmapSourceFromHBitmap(
                bmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        public void CreateImages(Document doc)
        {
            FilteredElementCollector collector;
            collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));
       
            foreach (FamilyInstance fi in collector)
            {
                ElementId typeId = fi.GetTypeId();
                ElementType type = doc.GetElement(typeId) as ElementType;
                System.Drawing.Size imgSize = new System.Drawing.Size(200, 200);
                //------------Prewiew Image
                
                Bitmap image = type.GetPreviewImage(imgSize);

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(ConvertBitmapToBitmapSource(image)));
                encoder.QualityLevel = 25;

                string TempImgFolder = System.IO.Path.GetTempPath() + "FamilyBrowser\\";
                if (!System.IO.Directory.Exists(TempImgFolder))
                {
                    System.IO.Directory.CreateDirectory(TempImgFolder);
                }
                string filename = TempImgFolder + type.Name + ".jpg";

                foreach (var fileimage in Directory.GetFiles(TempImgFolder))
                {
                    if (filename != fileimage)
                    {
                        FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
                        encoder.Save(file);
                        file.Close();
                      //  Process.Start(filename);
                    }
                }
            }
        }
         
        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Document doc = e.GetDocument();
            FilteredElementCollector families;
            Properties.Settings.Default.CollectedData = string.Empty;
            families = new FilteredElementCollector(doc).OfClass(typeof(Family));
            string temp = string.Empty;
          
            foreach (var item in families)
            {
                Family family = item as Family;
                FamilySymbol symbol;
                temp += item.Name;
                ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();
                foreach (ElementId id in familySymbolId)
                {
                    symbol = family.Document.GetElement(id) as FamilySymbol;
                    {                      
                        temp += "#" + symbol.Name;                       
                    }
                }
              //  CreateImages(doc);
                temp += "\n";
            }         
            Properties.Settings.Default.CollectedData = temp;
            TaskDialog.Show("Event + Settings", "Temp: " + temp);
        }
    }
}


