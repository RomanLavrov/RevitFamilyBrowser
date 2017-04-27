using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyBrowser
{
    public static class Tools
    {
        public static Workset GetActiveWorkset(Document doc)
        {
            WorksetTable table = doc.GetWorksetTable();
            WorksetId activeId = table.GetActiveWorksetId();
            Workset workset = table.GetWorkset(activeId);
            return workset;
        }

        private static FilteredElementCollector WorksetElements(Document doc, Workset workset)
        {
            FilteredElementCollector elementCollector = new FilteredElementCollector(doc).OfClass(typeof(Family));
            ElementWorksetFilter elementWorksetFilter = new ElementWorksetFilter(workset.Id, false);
            return elementCollector.WherePasses(elementWorksetFilter);
        }
        private static IList<Workset> GetAllWorksets(Document doc)
        {
            string message = string.Empty;
            FilteredWorksetCollector collector = new FilteredWorksetCollector(doc);
            collector.OfKind(WorksetKind.FamilyWorkset);
            IList<Workset> worksets = collector.ToWorksets();
            //if (worksets.Count == 0)
            //    TaskDialog.Show("Worksets", " No Worksets in project");
            foreach (Workset workset in worksets)
            {
                message += "Workset : " + workset.Name;
                message += "\nUnique Id : " + workset.UniqueId;
                message += "\nOwner : " + workset.Owner;
                message += "\nKind : " + workset.Kind;
                message += "\nIs default : " + workset.IsDefaultWorkset;
                message += "\nIs editable : " + workset.IsEditable;
                message += "\nIs open : " + workset.IsOpen;
                message += "\nIs visible by default : " + workset.IsVisibleByDefault + "\n";
                message += "\n\n";
                //TaskDialog.Show("GetWorksetsInfo", message);
            }
            return worksets;
        }

        private static string GetFamiliesElements(FilteredElementCollector elementCollector)
        {
            string temp = string.Empty;

            foreach (Element element in elementCollector)
            {
                if (!(element.Name.Contains("Standart") ||
                      element.Name.Contains("Mullion")))
                {
                    Family family = element as Family;
                    temp += element.Name;

                    ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();
                    foreach (ElementId id in familySymbolId)
                    {
                        var symbol = family.Document.GetElement(id) as FamilySymbol;
                        temp += "#" + symbol.Name;
                    }
                    temp += "\n";
                }
            }
            return temp;
        }

        public static void CollectFamilyData(Autodesk.Revit.DB.Document doc)
        {
            Properties.Settings.Default.CollectedData = string.Empty;

            if (GetAllWorksets(doc).Count == 0)
            {
                FilteredElementCollector elementCollector = new FilteredElementCollector(doc);
                elementCollector = elementCollector.OfClass(typeof(Family));
                Properties.Settings.Default.CollectedData = GetFamiliesElements(elementCollector);
            }
            else
            {
                string temp = string.Empty;
                foreach (Workset workset in GetAllWorksets(doc))
                {
                    if (workset.IsEditable)
                        temp += GetFamiliesElements(WorksetElements(doc, workset));
                }
                Properties.Settings.Default.CollectedData = temp;
            }
        }

        public static void CreateImages(Autodesk.Revit.DB.Document doc)
        {
            //TaskDialog.Show("Create Image", "Process Images");
            FilteredElementCollector collector;
            collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));
            int Instances = 0;

            foreach (FamilyInstance fi in collector)
            {
                try
                {
                    ElementId typeId = fi.GetTypeId();
                    ElementType type = doc.GetElement(typeId) as ElementType;
                    System.Drawing.Size imgSize = new System.Drawing.Size(200, 200);
                    Instances++;
                    //------------Prewiew Image-----
                    Bitmap image = type.GetPreviewImage(imgSize);

                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(ConvertBitmapToBitmapSource(image)));
                    encoder.QualityLevel = 25;

                    string TempImgFolder = System.IO.Path.GetTempPath() + "FamilyBrowser\\";
                    if (!System.IO.Directory.Exists(TempImgFolder))
                    {
                        System.IO.Directory.CreateDirectory(TempImgFolder);
                    }
                    string filename = TempImgFolder + type.Name + ".bmp";
                    FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    encoder.Save(file);
                    file.Close();
                }
                //TODO
                catch (Exception)
                {
                    ;
                }
            }
        }

        private static BitmapSource ConvertBitmapToBitmapSource(Bitmap bmp)
        {
            return System.Windows.Interop.Imaging
                .CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource GetImage(IntPtr bm)
        {
            BitmapSource bmSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bm,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return bmSource;
        }
    }
}
