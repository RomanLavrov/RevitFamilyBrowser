using System;
using System.Drawing;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using zRevitFamilyBrowser.WPF_Classes;
using Brushes = System.Windows.Media.Brushes;
using OperationCanceledException = Autodesk.Revit.Exceptions.OperationCanceledException;
using Point = System.Drawing.Point;

namespace zRevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        private const double FeetToMil = 25.4 * 12;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            var handler = new GridInstallEvent();
            var exEvent = ExternalEvent.Create(handler);

            var grid = new GridSetup(exEvent, handler);
            var window = WindowSetup(grid);

            //----------------------------------------------------------------------------------------

            var selection = uidoc.Selection;
            Room newRoom = null;
            //-----User select existing Room first-----
            if (selection.GetElementIds().Count > 0)
                foreach (var item in selection.GetElementIds())
                {
                    var elementType = doc.GetElement(item);
                    if (elementType.ToString() == typeof(Room).ToString())
                        newRoom = elementType as Room;
                }

            using (var transaction = new Transaction(doc, "Get room parameters"))
            {
                transaction.Start();
                var view = doc.ActiveView;
                if (newRoom == null)
                    try
                    {
                        if (uidoc.ActiveView.SketchPlane == null || view.GenLevel == null)
                        {
                            TaskDialog.Show("Section View", "Please switch to level view.");
                            return Result.Cancelled;
                        }

                        var point = selection.PickPoint("Point to create a room");
                        newRoom = doc.Create.NewRoom(view.GenLevel, new UV(point.X, point.Y));
                        
                    }
                    catch (OperationCanceledException)
                    {
                        return Result.Cancelled;
                    }
                //----------------------------------------------------------------------------------------
                var box = newRoom.get_BoundingBox(view);
                if (box == null) return Result.Failed;

               
                PointF roomMin = new PointF();
                roomMin.X = (float) (box.Min.X * FeetToMil);
                roomMin.Y = (float) (box.Min.Y * FeetToMil);

                PointF roomMax = new PointF();
                roomMax.X = (float)(box.Max.X * FeetToMil);
                roomMax.Y = (float)(box.Max.Y * FeetToMil);

                var roomDimensions = new RoomDimensions();
                grid.Scale = roomDimensions.GetScale(roomMin, roomMax, grid.CanvasSize);
                grid.RevitWalls = roomDimensions.GetWalls(newRoom);
                if (grid.RevitWalls.Count == 0)
                {
                    TaskDialog.Show("Error", "Room not detected");
                    return Result.Cancelled;
                }
                grid.Derrivation = GetDerrivation(box, grid);

                var bBox = new WpfCoordinates();
                grid.BoundingBoxLines = bBox.GetBoundingBox(roomMin, roomMax, grid);

                SymbolPreselectCheck(window);
                grid.DrawWalls();

                transaction.RollBack();
            }

            grid.TextBoxScale.Text = "Scale 1: " + grid.Scale;

            return Result.Succeeded;
        }

        private PointF GetDerrivation(BoundingBoxXYZ box, GridSetup grid)
        {
            PointF derrivationPoint = new PointF();

            PointF roomMin = new PointF();
            roomMin.X = (int)(box.Min.X * FeetToMil);
            roomMin.Y = (int)(box.Min.Y * FeetToMil);

            PointF roomMax = new PointF();
            roomMax.X = (int)(box.Max.X * FeetToMil);
            roomMax.Y = (int)(box.Max.Y * FeetToMil);

            double centerRoomX = roomMin.X / grid.Scale + (roomMax.X / grid.Scale - roomMin.X / grid.Scale) / 2;
            double centerRoomY = roomMin.Y / grid.Scale + (roomMax.Y / grid.Scale - roomMin.Y / grid.Scale) / 2;

            derrivationPoint.X = (float)(grid.CanvasSize / 2 - centerRoomX);
            derrivationPoint.Y = (float)(grid.CanvasSize / 2 + centerRoomY);

            return derrivationPoint;
        }

        private Window WindowSetup(GridSetup grid)
        {
            var window = new Window();

            window.Width = grid.Width;
            window.Height = grid.Height + 50;
            window.ResizeMode = ResizeMode.NoResize;
            window.Content = grid;
            window.Background = Brushes.WhiteSmoke;
            window.Topmost = true;
            return window;
        }

        private void SymbolPreselectCheck(Window window)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.FamilyName) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.FamilySymbol))
                window.Show();
            else
                MessageBox.Show("Select  symbol from browser");
        }
    }
}