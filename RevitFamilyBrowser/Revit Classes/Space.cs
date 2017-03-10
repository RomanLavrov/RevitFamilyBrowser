using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using System.Windows;
using RevitFamilyBrowser.WPF_Classes;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        int derrivationX = 0;
        int derrivationY = 0;

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
                List<System.Windows.Shapes.Line> wallCoord = roomDimensions.GetWalls(newRoom);

                int CanvasSize = (int)grid.canvas.Width;
                int Scale = roomDimensions.GetScale(wallCoord, CanvasSize);

                System.Windows.Shapes.Line centerRoom = new System.Windows.Shapes.Line();
                centerRoom.X1 = 0;
                centerRoom.Y1 = 0;
                centerRoom.X2 = roomMin.X / Scale + (roomMax.X / Scale - roomMin.X / Scale) / 2;
                centerRoom.Y2 = roomMin.Y / Scale + (roomMax.Y / Scale - roomMin.Y / Scale) / 2;
                centerRoom.Stroke = System.Windows.Media.Brushes.Red;
                // grid.canvas.Children.Add(centerRoom); 

                derrivationX = (int)(CanvasSize / 2 - centerRoom.X2);
                derrivationY = (int)(CanvasSize / 2 + centerRoom.Y2);

                window.Show();

                grid.textBox.Text = "Scale 1: " + Scale.ToString();

                foreach (var item in wallCoord)
                {
                    System.Windows.Shapes.Line myLine = new System.Windows.Shapes.Line();
                    myLine.X1 = (item.X1 / Scale) + derrivationX;
                    myLine.Y1 = ((-item.Y1 / Scale) + derrivationY);
                    myLine.X2 = (item.X2 / Scale) + derrivationX;
                    myLine.Y2 = ((-item.Y2 / Scale) + derrivationY);
                    myLine.Stroke = System.Windows.Media.Brushes.Black;
                    myLine.StrokeThickness = 3;

                    myLine.StrokeEndLineCap = PenLineCap.Round;
                    myLine.StrokeStartLineCap = PenLineCap.Round;

                    myLine.MouseDown += new MouseButtonEventHandler(line_MouseDown);
                    myLine.MouseUp += new MouseButtonEventHandler(line_MouseUp);
                    myLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                    myLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                    grid.canvas.Children.Add(myLine);
                }
                transaction.RollBack();
            }

            void line_MouseEnter(object sender, MouseEventArgs e)
            {
                Coordinates coord = new Coordinates();
                coord.Xstart = (int)((System.Windows.Shapes.Line)sender).X1;
                coord.Ystart = (int)((System.Windows.Shapes.Line)sender).Y1;
                coord.Xend = (int)((System.Windows.Shapes.Line)sender).X2;
                coord.Yend = (int)((System.Windows.Shapes.Line)sender).Y2;

                string LineData = string.Format("Line Length: {0}\nLine Slope: {1}", coord.GetLength((System.Windows.Shapes.Line)sender).ToString(), coord.GetSlope((System.Windows.Shapes.Line)sender).ToString());
                //  MessageBox.Show(LineData);

                ((System.Windows.Shapes.Line)sender).Stroke = Brushes.Gray;

            }

            void line_MouseLeave(object sender, MouseEventArgs e)
            {
                if (((System.Windows.Shapes.Line)sender).Stroke != Brushes.Red)
                {
                    ((System.Windows.Shapes.Line)sender).Stroke = Brushes.Black;
                }
            }

            void line_MouseUp(object sender, MouseButtonEventArgs e)
            {
                // Change line colour back to normal 
                ((System.Windows.Shapes.Line)sender).Stroke = System.Windows.Media.Brushes.Red;
            }

            void line_MouseDown(object sender, MouseButtonEventArgs e)
            {
                System.Windows.Shapes.Line line = (System.Windows.Shapes.Line)sender;
                line.Stroke = Brushes.Red;
                Coordinates coord = new Coordinates();
                
                foreach (var item in coord.DrawPerp(line, coord.SplitLine(line, 5)))
                {
                    grid.canvas.Children.Add(item);
                }
                
                //grid.canvas.Children.Add(coord.DrawPerpandicularA(line));
                //grid.canvas.Children.Add(coord.DrawPerpandicularB(line));
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