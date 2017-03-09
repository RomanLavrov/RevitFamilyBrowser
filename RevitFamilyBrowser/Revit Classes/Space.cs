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

            GridSetup grid = new GridSetup();
            Window window = new Window();
            window.Width = 1280;
            window.Height = 720;
            window.Content = grid;
            window.Background = System.Windows.Media.Brushes.WhiteSmoke;
            window.Topmost = true;

            RoomFilter filter = new RoomFilter();
            RoomDimensions roomDimensions = new RoomDimensions();
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
                View view = doc.ActiveView;
                Room newRoom = doc.Create.NewRoom(view.GenLevel, new UV(point.X, point.Y));

                BoundingBoxXYZ box = newRoom.get_BoundingBox(view);

                Coordinates center = new Coordinates();
                center.Xstart = (int)(box.Min.X - box.Max.X) / 2;
                center.Ystart = (int)(box.Min.Y - box.Max.Y) / 2;
                XYZ RoomCenterMin = box.Min;
                XYZ RoomCenterMax = box.Max;
                ConversionPoint roomMin = new ConversionPoint(RoomCenterMin);
                ConversionPoint roomMax = new ConversionPoint(RoomCenterMax);

                Location location = newRoom.Location;
                LocationPoint locationPoint = location as LocationPoint;
                XYZ roomPoint = locationPoint.Point;              
                List<Coordinates> wallCoord = roomDimensions.GetWalls(newRoom);

                window.Show();
                int CanvasSize = (int)grid.canvas.Width;
                int Scale = roomDimensions.GetScale(wallCoord, CanvasSize);

                //System.Windows.Shapes.Line centerLineX = new System.Windows.Shapes.Line();
                //centerLineX.X1 = 0;
                //centerLineX.Y1 = CanvasSize/2;
                //centerLineX.X2 = CanvasSize;
                //centerLineX.Y2 = CanvasSize/2;
                //centerLineX.Stroke = System.Windows.Media.Brushes.Red;
                //   grid.canvas.Children.Add(centerLineX);

                Coordinates centerLineY = new Coordinates();              
                centerLineY.Xstart = CanvasSize / 2;
                centerLineY.Ystart = 0;
                centerLineY.Xend = (CanvasSize / 2);
                centerLineY.Yend = CanvasSize;
                grid.canvas.Children.Add(roomDimensions.DrawDashedLine(centerLineY));

                System.Windows.Shapes.Line centerRoom = new System.Windows.Shapes.Line();
                centerRoom.X1 = 0;
                centerRoom.Y1 = 0;
                centerRoom.X2 = roomMin.X / Scale + (roomMax.X / Scale - roomMin.X / Scale) / 2;
                centerRoom.Y2 = roomMin.Y / Scale + (roomMax.Y / Scale - roomMin.Y / Scale) / 2;
                centerRoom.Stroke = System.Windows.Media.Brushes.Red;
                // grid.canvas.Children.Add(centerRoom); 

                int derrivationX = (int)(CanvasSize / 2 - centerRoom.X2);
                int derrivationY = (int)(CanvasSize / 2 + centerRoom.Y2);

                grid.textBox.Text = "Scale 1: " + Scale.ToString();

                foreach (var item in wallCoord)
                {
                    System.Windows.Shapes.Line myLine = new System.Windows.Shapes.Line();
                    myLine.X1 = (item.Xstart / Scale) + derrivationX;
                    myLine.Y1 = ((-item.Ystart / Scale) + derrivationY);
                    myLine.X2 = (item.Xend / Scale) + derrivationX;
                    myLine.Y2 = ((-item.Yend / Scale) + derrivationY);
                    myLine.Stroke = System.Windows.Media.Brushes.Black;

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

            return Result.Succeeded;
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

    }
}