using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using System.Windows;
using RevitFamilyBrowser.WPF_Classes;
using System.Drawing;
using System.Windows.Media;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;
            GridSetup grid = new GridSetup();
            Window window = new Window();
            window.Width = 1200;
            window.Height = 800;
            window.Content = grid;


            RoomFilter filter = new RoomFilter();

            using (var transaction = new Transaction(doc, "Family Symbol Collecting"))
            {
                transaction.Start();
                List<ElementId> SelectedRoom = null;
                Selection selection = uidoc.Selection;
                XYZ point;

                try
                {
                    point = selection.PickPoint("Point to create a room");
                }
                catch (Exception)
                {
                    throw;
                }

                Room newRoom = doc.Create.NewRoom(doc.ActiveView.GenLevel, new UV(point.X, point.Y));
                BoundingBoxXYZ box = newRoom.get_BoundingBox(view);


                Coordinates center = new Coordinates();
                center.Xstart = (int)(box.Min.X - box.Max.X) / 2;
                center.Ystart = (int)(box.Min.Y - box.Max.Y) / 2;
                XYZ RoomCenterMin = box.Min;
                XYZ RoomCenterMax = box.Max;
                ConversionPoint roomMin = new ConversionPoint(RoomCenterMin);
                ConversionPoint roomMax = new ConversionPoint(RoomCenterMax);
             //   TaskDialog.Show("Box", box.Min.ToString());


                Location location = newRoom.Location;
                LocationPoint locationPoint = location as LocationPoint;
                XYZ roomPoint = locationPoint.Point;

                SpatialElementBoundaryOptions boundaryOption = new SpatialElementBoundaryOptions();
                boundaryOption.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                IList<IList<BoundarySegment>> boundary = newRoom.GetBoundarySegments(boundaryOption);

                string temp = string.Empty;
                int WallNumber = 0;
                int SegmentNumber = 0;
                List<Coordinates> wallCoord = new List<Coordinates>();
                XYZ start = null;
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
                window.Show();
                int CanvasSize = 800;
                int Scale = GetScale(wallCoord, CanvasSize);
                Scale = 100;

                System.Windows.Shapes.Line centerLineX = new System.Windows.Shapes.Line();
                centerLineX.X1 = 0;
                centerLineX.Y1 = 400;
                centerLineX.X2 = 800;
                centerLineX.Y2 = 400;
                centerLineX.Stroke = System.Windows.Media.Brushes.Red;
                grid.canvas.Children.Add(centerLineX);

                System.Windows.Shapes.Line centerLineY = new System.Windows.Shapes.Line();
                centerLineY.X1 = 400;
                centerLineY.Y1 = 000;
                centerLineY.X2 = 400;
                centerLineY.Y2 = 800;
                centerLineY.Stroke = System.Windows.Media.Brushes.Red;
                grid.canvas.Children.Add(centerLineY);

                System.Windows.Shapes.Line centerRoom = new System.Windows.Shapes.Line();
                centerRoom.X1 = 0;
                centerRoom.Y1 = 0;
                centerRoom.X2 = roomMin.X / Scale + (roomMax.X / Scale - roomMin.X / Scale) / 2;
                centerRoom.Y2 = -roomMin.Y / Scale + (roomMax.Y / Scale - roomMin.Y / Scale) / 2;
                centerRoom.Stroke = System.Windows.Media.Brushes.Red;
                grid.canvas.Children.Add(centerRoom);

                int derrivationX = (int)(CanvasSize/2 - centerRoom.X2 );
                int derrivationY = (int)(-CanvasSize / 2 -  centerRoom.Y2);
              
                grid.textBox.Text = "Scale: " + Scale.ToString();

                foreach (var item in wallCoord)
                {
                    System.Windows.Shapes.Line myLine = new System.Windows.Shapes.Line();
                    myLine.X1 = (item.Xstart / Scale)  + derrivationX;
                    myLine.Y1 = (-item.Ystart / Scale) - derrivationY;
                    myLine.X2 = (item.Xend / Scale)  + derrivationX;
                    myLine.Y2 = (-item.Yend / Scale) - derrivationY;
                    myLine.Stroke = System.Windows.Media.Brushes.Red;
                    grid.canvas.Children.Add(myLine);
                }
                transaction.RollBack();
            }

            ////-----User select the Room first-----
            //if (selection.GetElementIds().Count > 0)
            //{
            //    foreach (var item in selection.GetElementIds())
            //    {
            //        //if (!(item.GetType() == typeof(Room)))
            //        //{
            //        //    TaskDialog.Show("Please select room", "Only Room can be selected");
            //        //    return Result.Failed;
            //        //}

            //        if (SelectedRoom == null)
            //        {
            //            SelectedRoom = new List<ElementId>(1);
            //        }
            //        SelectedRoom.Add(item);
            //    }
            //}

            ////-----Ask user to select the Room-----
            //if (SelectedRoom == null)
            //{
            //    IList<Reference> reference = null;
            //    try
            //    {
            //        reference = selection.PickObjects(ObjectType.Element, new RoomSelectionFilter(), "Please select room");
            //    }
            //    catch (Exception)
            //    {
            //        return Result.Cancelled;
            //    }
            //    SelectedRoom = new List<ElementId>(reference.Select(r => r.ElementId));
            //}

            //foreach (var item in SelectedRoom)
            //{
            //    Element e = doc.GetElement(item);
            //    BuildingAnalysis roomProcessing = GetRoom(e as Room);

            //    int nLoops = roomProcessing.Count;

            //    TaskDialog.Show("Room", e.Category + e.Id.ToString());

            //    int i = 0;

            //    foreach (RoomAnalysis room in roomProcessing)
            //    {
            //        TaskDialog.Show("RoomData", string.Format("  {0}: {1}", i++, room.ToString()));

            //    }

            //}
            return Result.Succeeded;
        }


        public int GetScale(List<Coordinates> wallCoord, int CanvasSize)
        {
            double LongestWall = 0;
            int Scale = 0;

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

        public BuildingAnalysis GetRoom(Room room)
        {
            SpatialElementBoundaryOptions boundaryOption = new SpatialElementBoundaryOptions();
            boundaryOption.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;

            IList<IList<BoundarySegment>> wallSegment = room.GetBoundarySegments(boundaryOption);

            int nLoops = wallSegment.Count;

            BuildingAnalysis buildingData = new BuildingAnalysis(nLoops);

            foreach (IList<BoundarySegment> segment in wallSegment)
            {
                int nSegments = segment.Count;
                RoomAnalysis roomData = new RoomAnalysis(nSegments);

                XYZ start = null;
                XYZ segmentStart = null; ///
                XYZ segmentEnd = null;

                foreach (BoundarySegment segmentPart in wallSegment)
                {

                    start = segmentPart.GetCurve().GetEndPoint(0);
                    roomData.Add(new ConversionPoint(start));

                    if (segmentEnd == null || segmentEnd.IsAlmostEqualTo(start))
                    {
                        TaskDialog.Show("Warning", "Last point to close with start");
                    }

                    segmentEnd = segmentPart.GetCurve().GetEndPoint(1);

                    if (start == null)
                    {
                        start = segmentStart;
                    }
                }
                buildingData.Add(roomData);
            }
            return buildingData;
        }
    }

    public class RoomSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Name == "Room")
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class RoomAnalysis : List<ConversionPoint>
    {
        public RoomAnalysis(int capacity) : base(capacity) { }

        public override string ToString()
        {
            return string.Join(", ", this);
        }
    }

    public class BuildingAnalysis : List<RoomAnalysis>
    {
        public BuildingAnalysis(int capacity) : base(capacity) { }
    }
}