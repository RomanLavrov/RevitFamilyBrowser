using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using Autodesk.Revit.Attributes;
using System.Diagnostics;
using Autodesk.Revit.DB.Events;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            app.DocumentChanged += new System.EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);

            CreateImages(doc);
            string name = "CustomCtrl_%CustomCtrl_%Familien Browser%Familien Browser";
            RevitCommandId id_addin = RevitCommandId.LookupCommandId(name);
            uiapp.PostCommand(id_addin);

            return Result.Succeeded;
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
          //  TaskDialog.Show("Event + Settings", "Temp: " + temp);
        }
        public void CreateImages(Document doc)
        {
           
            FilteredElementCollector collector;
            collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));

            foreach (FamilyInstance fi in collector)
            {
                TaskDialog.Show("Ok", "checked");
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
                string filename = TempImgFolder + type.Name + ".bmp";

             //   foreach (var fileimage in Directory.GetFiles(TempImgFolder))
                {
                    //if (filename != fileimage)
                    {
                        FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
                        encoder.Save(file);
                        file.Close();

                       // Process.Start(filename);
                    }
                }
            }
        }
    }
}
