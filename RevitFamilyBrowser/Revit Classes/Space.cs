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
using System.Linq;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        int derrivationX = 0;
        int derrivationY = 0;
        List<System.Windows.Shapes.Line> BoundingBox;
        List<List<System.Windows.Shapes.Line>> wallNormals;

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

            Selection selection = uidoc.Selection;
            Room newRoom = null;
            //-----User select the Room first-----
            if (selection.GetElementIds().Count > 0)
            {
                foreach (var item in selection.GetElementIds())
                {
                    Element elementType = doc.GetElement(item);
                    if ((elementType.ToString() == typeof(Room).ToString()))
                    {
                        newRoom = elementType as Room;
                    }
                }
            }
            
            using (var transaction = new Transaction(doc, "Family Symbol Collecting"))
            {
                transaction.Start();              
                View view = doc.ActiveView;
                if (newRoom == null)
                {
                    try
                    {
                        XYZ point = selection.PickPoint("Point to create a room");
                        newRoom = doc.Create.NewRoom(view.GenLevel, new UV(point.X, point.Y));
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {                       
                        return Result.Cancelled;
                    }
                    
                }

                BoundingBoxXYZ box = newRoom.get_BoundingBox(view);
                ConversionPoint roomMin = new ConversionPoint(box.Min);
                ConversionPoint roomMax = new ConversionPoint(box.Max);
                RoomDimensions roomDimensions = new RoomDimensions();

                int CanvasSize = (int)grid.canvas.Width;
                int Scale = roomDimensions.GetScale(roomMin, roomMax, CanvasSize);

                System.Windows.Shapes.Line centerRoom = new System.Windows.Shapes.Line();
                centerRoom.X1 = 0;
                centerRoom.Y1 = 0;
                centerRoom.X2 = roomMin.X / Scale + (roomMax.X / Scale - roomMin.X / Scale) / 2;
                centerRoom.Y2 = roomMin.Y / Scale + (roomMax.Y / Scale - roomMin.Y / Scale) / 2;
                centerRoom.Stroke = System.Windows.Media.Brushes.Red;
                // grid.canvas.Children.Add(centerRoom); 
                derrivationX = (int)(CanvasSize / 2 - centerRoom.X2);
                derrivationY = (int)(CanvasSize / 2 + centerRoom.Y2);

                ProcessCoordinates bBox = new ProcessCoordinates();
                BoundingBox = bBox.GetBoundingBox(roomMin, roomMax, Scale, derrivationX, derrivationY);

                window.Show();
                grid.textBox.Text = "Scale 1: " + Scale.ToString();
                grid.buttonReset.Click += buttonReset_Click;

                List<System.Windows.Shapes.Line> wallCoord = roomDimensions.GetWalls(newRoom);
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

                ProcessCoordinates coord = new ProcessCoordinates();
                List<System.Drawing.Point> listPointsOnWall;
                if (grid.radioEqual.IsChecked == true)
                {
                    listPointsOnWall = coord.SplitLine(line, Convert.ToInt32(grid.textBoxHorizontal.Text));
                }
                else
                    listPointsOnWall = coord.SplitLineProportional(line, Convert.ToInt32(grid.textBoxHorizontal.Text));

                List<System.Windows.Shapes.Line> listPerpendiculars = coord.DrawPerp(line, listPointsOnWall);
                wallNormals.Add(listPerpendiculars);
                
                foreach (var item in listPerpendiculars)
                {
                    grid.canvas.Children.Add(coord.BuildBoundedLine(BoundingBox, item));
                }
                List<System.Drawing.Point> nolmalsIntersectionPoints = new List<System.Drawing.Point>();

                //if (wallNormals.Count > 1)
                //{
                //    foreach (var wall in wallNormals)
                //    {
                //        foreach (var normal1 in wall)
                //        {
                //            foreach (var normal2 in wall)
                //            {
                //                coord.GetIntersection(normal1, normal2);
                //                nolmalsIntersectionPoints.Add(coord.GetIntersection(normal1, normal2));
                //            }
                //        }
                //    }
                //}
               
                //MessageBox.Show("Total intersection = " + nolmalsIntersectionPoints.Count);
            }

            void buttonReset_Click(object sender, RoutedEventArgs e)
            {
                List<System.Windows.Shapes.Line> lines = grid.canvas.Children.OfType<System.Windows.Shapes.Line>().Where(r => r.Stroke == Brushes.SteelBlue).ToList();

                foreach (var item in lines)
                {
                    grid.canvas.Children.Remove(item);
                }
            }
            return Result.Succeeded;
        }
    }
}