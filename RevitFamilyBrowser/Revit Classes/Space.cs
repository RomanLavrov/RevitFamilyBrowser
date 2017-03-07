using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            List<ElementId> SelectedRoom = null;
            Selection selection = uidoc.Selection;

            RoomFilter filter = new RoomFilter();

            using (var transaction = new Transaction(doc, "Family Symbol Collecting"))
            {
                transaction.Start();

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

                Location location = newRoom.Location;
                LocationPoint locationPoint = location as LocationPoint;
                XYZ roomPoint = locationPoint.Point;

                SpatialElementBoundaryOptions boundaryOption = new SpatialElementBoundaryOptions();
                boundaryOption.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                IList<IList<BoundarySegment>> boundary = newRoom.GetBoundarySegments(boundaryOption);

                string temp = string.Empty;
                int WallNumber = 0;
                int SegmentNumber = 0;

                XYZ start = null;
                XYZ segmentStart = null; ///
                XYZ segmentEnd = null;
               
                int nLoops = boundary.Count;
                foreach (IList<BoundarySegment> wall in boundary)
                {
                    WallNumber++;
                    foreach (BoundarySegment segment in wall)
                    {
                        segmentStart = segment.GetCurve().GetEndPoint(0);
                        ConversionPoint Start = new ConversionPoint(segmentStart);
                       
                        segmentEnd = segment.GetCurve().GetEndPoint(1);
                        ConversionPoint End = new ConversionPoint(segmentEnd);

                        SegmentNumber++;                       
                        temp += "WallNumber:" + WallNumber + " " + "SegmentNumber:" + SegmentNumber + " " + Start.ToString() + End.ToString() + "\n";
                    }
                }
                TaskDialog.Show("Boundaries", temp);
                
                    transaction.Commit();
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