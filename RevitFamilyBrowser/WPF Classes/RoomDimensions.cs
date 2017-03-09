using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitFamilyBrowser.Revit_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RevitFamilyBrowser.WPF_Classes
{

    //This class transform Revit coordinates into WPF canvas coordinates

    class RoomDimensions
    {
        public int MyProperty { get; set; }

       public void GetBoundingBox(Room newRoom, View view)
        {
            BoundingBoxXYZ box = newRoom.get_BoundingBox(view);
            Coordinates center = new Coordinates();

            center.X = (int)(box.Min.X - box.Max.X) / 2;
            center.Y = (int)(box.Min.Y - box.Max.Y) / 2;
          
            ConversionPoint roomMin = new ConversionPoint(box.Min);
            ConversionPoint roomMax = new ConversionPoint(box.Max);
        }            

        //-----Get coordinates for all walls in Room
        public List<Coordinates> GetWalls(Room room)
        {
            SpatialElementBoundaryOptions boundaryOption = new SpatialElementBoundaryOptions();
            boundaryOption.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;

            IList<IList<BoundarySegment>> boundary = room.GetBoundarySegments(boundaryOption);

            string temp = string.Empty; //can be used to see Wall and segment numbers  
            int WallNumber = 0;
            int SegmentNumber = 0;
            List<Coordinates> wallCoord = new List<Coordinates>();           
            XYZ segmentStart = null; ///
            XYZ segmentEnd = null;

            int nLoops = boundary.Count;
            foreach (IList<BoundarySegment> wall in boundary)
            {
                WallNumber++;
                foreach (BoundarySegment segment in wall)
                {
                    Coordinates coord = new Coordinates();

                    segmentStart = segment.GetCurve().GetEndPoint(0);
                    ConversionPoint Start = new ConversionPoint(segmentStart);
                    coord.Xstart = Start.X;
                    coord.Ystart = Start.Y;

                    segmentEnd = segment.GetCurve().GetEndPoint(1);
                    ConversionPoint End = new ConversionPoint(segmentEnd);
                    coord.Xend = End.X;
                    coord.Yend = End.Y;

                    wallCoord.Add(coord);

                    SegmentNumber++;
                    temp += "WallNumber:" + WallNumber + " " + "SegmentNumber:" + SegmentNumber + " " + Start.ToString() + End.ToString() + "\n";
                }
            }
            //  TaskDialog.Show("Boundaries", temp);
            return wallCoord;
        }

        //-----Compare Longest Wall in Room with Canvas to fit room boundary into Canvas
        public int GetScale(List<Coordinates> wallCoord, int CanvasSize)
        {
            double LongestWall = 0;
            int Scale = 0;
            CanvasSize = CanvasSize - 5;
            foreach (var item in wallCoord)
            {
                Coordinates coord = new Coordinates();
                coord = item;
                coord.Length(item);

                if (coord.Length(item) > LongestWall)
                {
                    LongestWall = coord.Length(item);
                }

                if ((LongestWall / CanvasSize) < 1)
                {
                    Scale = 1;
                }
                else if ((LongestWall / CanvasSize) > 1 && (LongestWall / CanvasSize) < 2)
                {
                    Scale = 2;
                }
                else if ((LongestWall / CanvasSize) > 2 && (LongestWall / CanvasSize) < 5)
                {
                    Scale = 5;
                }
                else if ((LongestWall / CanvasSize) > 5 && (LongestWall / CanvasSize) < 10)
                {
                    Scale = 10;
                }
                else if ((LongestWall / CanvasSize) > 10 && (LongestWall / CanvasSize) < 20)
                {
                    Scale = 20;
                }
                else if ((LongestWall / CanvasSize) > 20 && (LongestWall / CanvasSize) < 25)
                {
                    Scale = 25;
                }
                else if ((LongestWall / CanvasSize) > 25 && (LongestWall / CanvasSize) < 50)
                {
                    Scale = 50;
                }
                else if ((LongestWall / CanvasSize) > 50 && (LongestWall / CanvasSize) < 100)
                {
                    Scale = 100;
                }
                else if ((LongestWall / CanvasSize) > 100 && (LongestWall / CanvasSize) < 200)
                {
                    Scale = 200;
                }
                else if ((LongestWall / CanvasSize) > 200 && (LongestWall / CanvasSize) < 500)
                {
                    Scale = 500;
                }
                else if ((LongestWall / CanvasSize) > 500 && (LongestWall / CanvasSize) < 1000)
                {
                    Scale = 1000;
                }
            }
            return Scale;
        }

        //-----Draw Center Line on Canvas by given Start and End points
        public System.Windows.Shapes.Line DrawCenterLine(Coordinates coord)
        {
            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.X1 = coord.Xstart;
            line.Y1 = coord.Ystart;
            line.X2 = coord.Xend;
            line.Y2 = coord.Yend;

            line.Stroke = System.Windows.Media.Brushes.Red;
            DoubleCollection dash = new DoubleCollection() {130 , 8, 15, 8 };           
            line.StrokeDashArray = dash;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            return line;
        }

        //-----Draw Dashed Line on Canvas by given Start and End points
        public System.Windows.Shapes.Line DrawDashedLine(Coordinates coord)
        {
            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.X1 = coord.Xstart;
            line.Y1 = coord.Ystart;
            line.X2 = coord.Xend;
            line.Y2 = coord.Yend;

            line.Stroke = System.Windows.Media.Brushes.Gray;
            DoubleCollection dash = new DoubleCollection() { 12, 12};
            line.StrokeDashArray = dash;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            return line;
        }
    }   
}
