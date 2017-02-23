using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]

    class Image : IExternalCommand
    {
        static BitmapSource ConvertBitmapToBitmapSource( Bitmap bmp)
        {
            return System.Windows.Interop.Imaging
              .CreateBitmapSourceFromHBitmap(
                bmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            collector.OfClass(typeof(FamilyInstance));

            foreach (FamilyInstance fi in collector)
            {
                Debug.Assert(null != fi.Category,
                  "expected family instance to have a valid category");

                ElementId typeId = fi.GetTypeId();
                ElementType type = doc.GetElement(typeId) as ElementType;

                System.Drawing.Size imgSize = new System.Drawing.Size(200, 200);
                Bitmap image = type.GetPreviewImage(imgSize);

                // encode image to jpeg for test display purposes:

                JpegBitmapEncoder encoder
                  = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(
                  ConvertBitmapToBitmapSource(image)));

                encoder.QualityLevel = 100;

                string filename = "a.jpg";

                FileStream file = new FileStream(
                  filename, FileMode.Create, FileAccess.Write);

                encoder.Save(file);
                file.Close();

                Process.Start(filename); // test display

            }

            return Result.Succeeded;
        }
    }
}


