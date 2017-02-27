using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitFamilyBrowser.WPF_Classes
{
    public class FamilyData
    {
        //public string Symbol { get; set; }
        // public Bitmap Bitmap { get; set; }
        public string FamilyName { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public Uri img { get; set; }
        public Uri familyImage { get; set; }
        //public ImageSource img
        //{
        //    get
        //    {
        //        return Imaging.CreateBitmapSourceFromHBitmap(
        //            Bitmap.GetHbitmap(),
        //            IntPtr.Zero,
        //            Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //    }
        //    set { }
        //}
    }
}
