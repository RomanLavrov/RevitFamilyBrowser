using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitFamilyBrowser.WPF_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RevitFamilyBrowser.Pattern_Elements_Install;
using Brushes = System.Windows.Media.Brushes;
using Line = System.Windows.Shapes.Line;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        public List<System.Drawing.Point> InsertCoord { get; set; }
        int derrivationX = 0;
        int derrivationY = 0;
        int Scale = 0;
        int CanvasSize = 0;
        private List<Line> revitWalls;
        private List<Line> wpfWalls;
        List<Line> BoundingBox;

        List<Line> revitWallNormals = new List<Line>();
        List<List<Line>> wallNormals = new List<List<Line>>();

        List<System.Drawing.Point> gridPoints = new List<System.Drawing.Point>();
        List<System.Drawing.PointF> rvtGridPoints = new List<PointF>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            GridInstallEvent handler = new GridInstallEvent();
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            GridSetup grid = new GridSetup(exEvent, handler);
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
                        var point = selection.PickPoint("Point to create a room");
                        newRoom = doc.Create.NewRoom(view.GenLevel, new UV(point.X, point.Y));
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return Result.Cancelled;
                    }
                }

                BoundingBoxXYZ box = newRoom.get_BoundingBox(view);
                var roomMin = new ConversionPoint(box.Min);
                var roomMax = new ConversionPoint(box.Max);
                RoomDimensions roomDimensions = new RoomDimensions();

                CanvasSize = (int)grid.canvas.Width;
                Scale = roomDimensions.GetScale(roomMin, roomMax, CanvasSize);

                System.Windows.Shapes.Line centerRoom = new System.Windows.Shapes.Line();
                centerRoom.X1 = 0;
                centerRoom.Y1 = 0;
                centerRoom.X2 = roomMin.X / Scale + (roomMax.X / Scale - roomMin.X / Scale) / 2;
                centerRoom.Y2 = roomMin.Y / Scale + (roomMax.Y / Scale - roomMin.Y / Scale) / 2;
                centerRoom.Stroke = System.Windows.Media.Brushes.Red;
                // grid.canvas.Children.Add(centerRoom); 
                derrivationX = (int)(CanvasSize / 2 - centerRoom.X2);
                derrivationY = (int)(CanvasSize / 2 + centerRoom.Y2);

                WpfCoordinates bBox = new WpfCoordinates();
                BoundingBox = bBox.GetBoundingBox(roomMin, roomMax, Scale, derrivationX, derrivationY);

                window.Show();
                grid.textBox.Text = "Scale 1: " + Scale.ToString();
                grid.buttonReset.Click += buttonReset_Click;

                revitWalls = roomDimensions.GetWalls(newRoom);
                wpfWalls = GetWpfWalls(revitWalls, derrivationX, derrivationY, Scale);
                Draw(wpfWalls);

                transaction.RollBack();
            }

            List<Line> GetWpfWalls(List<Line> revitWalls, int derrivationX, int derrivationY, int Scale)
            {
                List<Line> wpfWalls = new List<Line>();
                foreach (var item in revitWalls)
                {
                    System.Windows.Shapes.Line myLine = new System.Windows.Shapes.Line();
                    myLine.X1 = (item.X1 / Scale) + derrivationX;
                    myLine.Y1 = ((-item.Y1 / Scale) + derrivationY);
                    myLine.X2 = (item.X2 / Scale) + derrivationX;
                    myLine.Y2 = ((-item.Y2 / Scale) + derrivationY);
                    wpfWalls.Add(myLine);
                }
                return wpfWalls;
            }

            void line_MouseDown(object sender, MouseButtonEventArgs e)
            {
                Line line = (Line)sender;
                line.Stroke = Brushes.Red;

                int wallIndex = 0;
                foreach (var item in wpfWalls)
                {
                    if (sender.Equals(item))
                    {
                        wallIndex = wpfWalls.IndexOf(item);
                    }
                }

                List<System.Drawing.Point> listPointsOnWall;
                gridPoints.Clear();

                WpfCoordinates wpfCoord = new WpfCoordinates();
                if (grid.radioEqual.IsChecked == true)
                {
                    listPointsOnWall = wpfCoord.SplitLine(line, Convert.ToInt32(grid.textBoxHorizontal.Text));
                }
                else
                    listPointsOnWall = wpfCoord.SplitLineProportional(line, Convert.ToInt32(grid.textBoxHorizontal.Text));

                List<System.Windows.Shapes.Line> listPerpendiculars = wpfCoord.DrawPerp(line, listPointsOnWall);
                foreach (var item in listPerpendiculars)
                {
                    grid.canvas.Children.Add(wpfCoord.BuildBoundedLine(BoundingBox, item));
                }

                //Properties.Settings.Default.InstallPoints = string.Empty;

                gridPoints = wpfCoord.GetGridPoints(listPerpendiculars, wallNormals);
                grid.textBoxQuantity.Text = "Items: " + gridPoints.Count.ToString();
                foreach (var item in gridPoints)
                {
                    double x = ((((item.X - 0.5) * Scale) / 304.8) - derrivationX * Scale / 304.8);
                    double y = (((-(item.Y - 0.5) * Scale) / 304.8) + derrivationY * Scale / 304.8);
                }
                
                //----------------------------------------Revit coordinates--------------------------------------------------------
                CoordinatesRevit rvt = new CoordinatesRevit();
                Line rvtWall = revitWalls[wallIndex];
                List<System.Drawing.PointF> rvtPointsOnWall = rvt.GetSplitPoints(rvtWall, Convert.ToInt32(grid.textBoxHorizontal.Text));

                //MessageBox.Show($"Wall coors\nX1={rvtWall.X1}, Y1={rvtWall.Y1}\nX2={rvtWall.X2} Y2={rvtWall.Y2}");
                List<System.Windows.Shapes.Line> rvtListPerpendiculars =
                    rvt.GetPerpendiculars(rvtWall, rvtPointsOnWall);

                rvtGridPoints = rvt.GetGridPointsRvt(revitWallNormals, rvtListPerpendiculars);
                
                foreach (var item in rvtGridPoints)
                {
                    Properties.Settings.Default.InstallPoints += (item.X)/(25.4*12) + "*" + (item.Y)/(25.4*12) + "\n";
                }

                //------------------------------------Draw Lines to intersection points in wpf window------------------------------------------------------------------------
                //List<System.Drawing.Point> temp = new List<System.Drawing.Point>();
                //temp = coord.GetIntersectInRoom(BoundingBox, gridPoints);

                //string test = string.Empty;
                //int count = 0;
                //foreach (var item in temp)
                //{
                //    System.Windows.Shapes.Line intersect = new System.Windows.Shapes.Line();
                //    intersect.X1 = 0;
                //    intersect.Y1 = 0;
                //    intersect.X2 = item.X;
                //    intersect.Y2 = item.Y;
                //    count++;
                //    //test += count.ToString() + ". X=" + item.X.ToString() + " Y=" + item.Y.ToString() + "\n";
                //    intersect.Stroke = Brushes.Red;

                //    grid.canvas.Children.Add(intersect);
                //}
                //-----------------------------------------------------------------------------------------------------------------------
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

            void buttonReset_Click(object sender, RoutedEventArgs e)
            {
                List<System.Windows.Shapes.Line> lines = grid.canvas.Children.OfType<System.Windows.Shapes.Line>().Where(r => r.Stroke == Brushes.SteelBlue).ToList();

                foreach (var item in lines)
                {
                    grid.canvas.Children.Remove(item);
                }
            }

            void Draw(List<Line> wpfWalls)
            {
                foreach (Line myLine in wpfWalls)
                {
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
            }

            return Result.Succeeded;
        }
    }
}