using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitFamilyBrowser.Revit_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RevitFamilyBrowser.WPF_Classes
{

    //This class transform Revit coordinates into WPF canvas coordinates

    class RoomDimensions
    {
        //public void GetBoundingBox(Room newRoom, View view)
        //{
        //    BoundingBoxXYZ box = newRoom.get_BoundingBox(view);
        //    WpfCoordinates center = new WpfCoordinates();

        //    center.X = (int)(box.Min.X - box.Max.X) / 2;
        //    center.Y = (int)(box.Min.Y - box.Max.Y) / 2;

        //    ConversionPoint roomMin = new ConversionPoint(box.Min);
        //    ConversionPoint roomMax = new ConversionPoint(box.Max);
        //}


        public List<System.Windows.Shapes.Line> GetWalls(Room room)
        {
            SpatialElementBoundaryOptions boundaryOption = new SpatialElementBoundaryOptions();
            boundaryOption.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;

            IList<IList<BoundarySegment>> boundary = room.GetBoundarySegments(boundaryOption);
            List<System.Windows.Shapes.Line> wallCoord = new List<System.Windows.Shapes.Line>();

            foreach (IList<BoundarySegment> walls in boundary)
            {
                foreach (BoundarySegment segment in walls)
                {
                    System.Windows.Shapes.Line wall = new System.Windows.Shapes.Line();

                    var segmentStart = segment.GetCurve().GetEndPoint(0);

                    //wall.X1 = Convert.ToInt32(segmentStart.X * 25.4 * 12);
                    //wall.Y1 = Convert.ToInt32(segmentStart.Y * 25.4 * 12);
                    wall.X1 = (segmentStart.X * 25.4 * 12);
                    wall.Y1 = (segmentStart.Y * 25.4 * 12);

                    var segmentEnd = segment.GetCurve().GetEndPoint(1);

                    //wall.X2 = Convert.ToInt32(segmentEnd.X * 25.4 * 12);
                    //wall.Y2 = Convert.ToInt32(segmentEnd.Y * 25.4 * 12);
                    wall.X2 = (segmentEnd.X * 25.4 * 12);
                    wall.Y2 = (segmentEnd.Y * 25.4 * 12);

                    wallCoord.Add(wall);
                }
            }
            return wallCoord;
        }

        public int GetScale(PointF min, PointF max, int CanvasSize)
        {
            int BorderAroutWorkArea = 150;
            double LongestWall = 0;
            int Scale = 0;
            CanvasSize = CanvasSize - BorderAroutWorkArea;
            List<System.Windows.Shapes.Line> sides = new List<System.Windows.Shapes.Line>();

            System.Windows.Shapes.Line sideA = new System.Windows.Shapes.Line();
            sideA.X1 = min.X; sideA.Y1 = min.Y;
            sideA.X2 = min.X; sideA.Y2 = max.Y;
            sides.Add(sideA);

            System.Windows.Shapes.Line sideB = new System.Windows.Shapes.Line();
            sideB.X1 = min.X; sideB.Y1 = max.Y;
            sideB.X2 = max.X; sideB.Y2 = max.Y;
            sides.Add(sideB);

            System.Windows.Shapes.Line sideC = new System.Windows.Shapes.Line();
            sideC.X1 = max.X; sideC.Y1 = max.Y;
            sideC.X2 = max.X; sideC.Y2 = min.Y;
            sides.Add(sideC);

            System.Windows.Shapes.Line sideD = new System.Windows.Shapes.Line();
            sideD.X1 = max.X; sideD.Y1 = min.Y;
            sideD.X2 = min.X; sideD.Y2 = min.Y;
            sides.Add(sideD);
            WpfCoordinates coord = new WpfCoordinates();

            foreach (var item in sides)
            {
                if (coord.GetLength(item) > LongestWall)
                    LongestWall = coord.GetLength(item);
            }

            if ((LongestWall / CanvasSize) < 1)
            {
                Scale = 1;
            }
            else if ((LongestWall / CanvasSize) >= 1 && (LongestWall / CanvasSize) < 2)
            {
                Scale = 2;
            }
            else if ((LongestWall / CanvasSize) >= 2 && (LongestWall / CanvasSize) < 5)
            {
                Scale = 5;
            }
            else if ((LongestWall / CanvasSize) >= 5 && (LongestWall / CanvasSize) < 10)
            {
                Scale = 10;
            }
            else if ((LongestWall / CanvasSize) >= 10 && (LongestWall / CanvasSize) < 20)
            {
                Scale = 20;
            }
            else if ((LongestWall / CanvasSize) >= 20 && (LongestWall / CanvasSize) < 25)
            {
                Scale = 25;
            }
            else if ((LongestWall / CanvasSize) >= 25 && (LongestWall / CanvasSize) < 50)
            {
                Scale = 50;
            }
            else if ((LongestWall / CanvasSize) >= 50 && (LongestWall / CanvasSize) < 100)
            {
                Scale = 100;
            }
            else if ((LongestWall / CanvasSize) >= 100 && (LongestWall / CanvasSize) < 200)
            {
                Scale = 200;
            }
            else if ((LongestWall / CanvasSize) >= 200 && (LongestWall / CanvasSize) < 500)
            {
                Scale = 500;
            }
            else if ((LongestWall / CanvasSize) >= 500 && (LongestWall / CanvasSize) < 1000)
            {
                Scale = 1000;
            }

            return Scale;
        }
    }
}
