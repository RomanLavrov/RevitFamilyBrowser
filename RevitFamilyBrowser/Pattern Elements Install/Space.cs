using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitFamilyBrowser.Revit_Classes;
using RevitFamilyBrowser.WPF_Classes;
using Brushes = System.Windows.Media.Brushes;
using Line = System.Windows.Shapes.Line;
using RevitFamilyBrowser.Pattern_Elements_Install;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        private System.Drawing.Point derrivation;
        int CanvasSize;


        //List<Line> revitWallNormals = new List<Line>();

        //List<System.Drawing.Point> gridPoints = new List<System.Drawing.Point>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            GridInstallEvent handler = new GridInstallEvent();
            ExternalEvent exEvent = ExternalEvent.Create(handler);
           


            GridSetup grid = new GridSetup(exEvent, handler);
            Window window = WindowSetup(grid);

            CanvasSize = (int)grid.canvas.Width;
            //----------------------------------------------------------------------------------------
            RoomDimensions roomDimensions = new RoomDimensions();
            Selection selection = uidoc.Selection;
            Room newRoom = null;
            //-----User select existing Room first-----
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

            using (var transaction = new Transaction(doc, "Get room parameters"))
            {
                transaction.Start();
                View view = doc.ActiveView;
                if (newRoom == null)
                {
                    try
                    {
                        if (uidoc.ActiveView.SketchPlane == null)
                        {
                            TaskDialog.Show("Section View", "Please switch to level view.");
                            return Result.Failed;
                        }
                        var point = selection.PickPoint("Point to create a room");

                        if (view.GenLevel == null)
                        {
                            TaskDialog.Show("3D View", "Please switch to level view.");
                            return Result.Cancelled;
                        }
                        newRoom = doc.Create.NewRoom(view.GenLevel, new UV(point.X, point.Y));
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return Result.Cancelled;
                    }
                }
                //----------------------------------------------------------------------------------------
                BoundingBoxXYZ box = newRoom.get_BoundingBox(view);
                if (box == null) { return Result.Failed; }
                var roomMin = new ConversionPoint(box.Min);
                var roomMax = new ConversionPoint(box.Max);

                // Scale = roomDimensions.GetScale(roomMin, roomMax, CanvasSize);
                grid.SCALE = roomDimensions.GetScale(roomMin, roomMax, CanvasSize);
                derrivation = GetDerrivation(box, grid.SCALE);

                WpfCoordinates bBox = new WpfCoordinates();
                // BoundingBox = bBox.GetBoundingBox(roomMin, roomMax, Scale, derrivation.X, derrivation.Y);
                grid.BoundingBoxLines = bBox.GetBoundingBox(roomMin, roomMax, grid.SCALE, derrivation.X, derrivation.Y);

                SymbolPreselectCheck(window);
                // revitWalls = roomDimensions.GetWalls(newRoom);
                grid.RevitWalls = roomDimensions.GetWalls(newRoom);
                // wpfWalls = grid.GetWpfWalls(revitWalls, derrivation.X, derrivation.Y, Scale);
                grid.WpfWalls = grid.GetWpfWalls(grid.RevitWalls, derrivation.X, derrivation.Y, grid.SCALE);
                grid.Draw(grid.WpfWalls);
                //Draw(wpfWalls);

                transaction.RollBack();
            }

            grid.TextBoxScale.Text = "Scale 1: " + grid.SCALE.ToString();

            grid.buttonReset.Click += grid.buttonReset_Click;

            //void line_MouseDown(object sender, MouseButtonEventArgs e)
            //{
            //    Line line = (Line)sender;
            //    line.Stroke = Brushes.Red;

            //    int wallIndex = 0;
            //    foreach (var item in grid.WpfWalls)
            //    //     foreach (var item in wpfWalls)
            //    {
            //        if (sender.Equals(item))
            //            // wallIndex = wpfWalls.IndexOf(item);
            //            wallIndex = grid.WpfWalls.IndexOf(item);
            //    }

            //    List<System.Drawing.Point> listPointsOnWall = grid.GetListPointsOnWall(line, out string InstallType);

            //    WpfCoordinates wpfCoord = new WpfCoordinates();
            //    List<Line> listPerpendiculars = wpfCoord.DrawPerp(line, listPointsOnWall);
            //    foreach (var item in listPerpendiculars)
            //    {
            //        grid.canvas.Children.Add(wpfCoord.BuildBoundedLine(grid.BoundingBoxLines, item));
            //        //  grid.canvas.Children.Add(wpfCoord.BuildBoundedLine(BoundingBox, item));
            //        //TODO arrange label with size -------------------------------------------
            //        System.Windows.Controls.Label WallDimension = new Label();
            //        WallDimension.Content = (wpfCoord.GetLength(line) * grid.SCALE);
            //        Canvas.SetLeft(WallDimension, item.X2);
            //        Canvas.SetTop(WallDimension, item.Y2);
            //        grid.canvas.Children.Add(WallDimension);
            //        //--------------------------------------------------
            //    }
            //    gridPoints.Clear();

            //    gridPoints = wpfCoord.GetGridPoints(listPerpendiculars);
            //    grid.textBoxQuantity.Text = "Items: " + gridPoints.Count;
            //    grid.GetRevitInstallCoordinates(revitWallNormals, grid.RevitWalls, wallIndex, InstallType);
            //}

            //void Draw(List<Line> wpfWalls)
            //{
            //    foreach (Line myLine in wpfWalls)
            //    {
            //        myLine.Stroke = System.Windows.Media.Brushes.Black;
            //        myLine.StrokeThickness = 2;

            //        myLine.StrokeEndLineCap = PenLineCap.Round;
            //        myLine.StrokeStartLineCap = PenLineCap.Round;

            //        myLine.MouseDown += new MouseButtonEventHandler(grid.line_MouseDown);
            //        myLine.MouseUp += new MouseButtonEventHandler(grid.line_MouseUp);
            //        myLine.MouseEnter += new MouseEventHandler(grid.line_MouseEnter);
            //        myLine.MouseLeave += new MouseEventHandler(grid.line_MouseLeave);
            //        grid.canvas.Children.Add(myLine);
            //    }
            //}

            return Result.Succeeded;
        }

        private System.Drawing.Point GetDerrivation(BoundingBoxXYZ box, int scale)
        {
            System.Drawing.Point DerrivationPoint = new System.Drawing.Point();

            var roomMin = new ConversionPoint(box.Min);
            var roomMax = new ConversionPoint(box.Max);

            double centerRoomX = roomMin.X / scale + (roomMax.X / scale - roomMin.X / scale) / 2;
            double centerRoomY = roomMin.Y / scale + (roomMax.Y / scale - roomMin.Y / scale) / 2;

            DerrivationPoint.X = (int)(CanvasSize / 2 - centerRoomX);
            DerrivationPoint.Y = (int)(CanvasSize / 2 + centerRoomY);

            return DerrivationPoint;
        }

        private Window WindowSetup(GridSetup grid)
        {
            Window window = new Window();

            window.Width = grid.Width;
            window.Height = grid.Height + 50;
            window.ResizeMode = ResizeMode.NoResize;
            window.Content = grid;
            window.Background = System.Windows.Media.Brushes.WhiteSmoke;
            window.Topmost = true;
            return window;
        }

        private void SymbolPreselectCheck(Window window)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.FamilyName) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.FamilySymbol))
            {
                window.Show();
            }
            else
                MessageBox.Show("Select  symbol from browser");
        }
    }
}