using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitFamilyBrowser.WPF_Classes;
using System;
using System.Windows;

namespace RevitFamilyBrowser.Revit_Classes
{
    [Transaction(TransactionMode.Manual)]
    public class Space : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            GridInstallEvent handler = new GridInstallEvent();
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            GridSetup grid = new GridSetup(exEvent, handler);
            Window window = WindowSetup(grid);
            
            //----------------------------------------------------------------------------------------

            Selection selection = uidoc.Selection;
            Room newRoom = null;
            //-----User select existing Room first-----
            if (selection.GetElementIds().Count > 0)
            {
                foreach (var item in selection.GetElementIds())
                {
                    Element elementType = doc.GetElement(item);
                    if (elementType.ToString() == typeof(Room).ToString())
                        newRoom = elementType as Room;
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
                        if (uidoc.ActiveView.SketchPlane == null || view.GenLevel == null)
                        {
                            TaskDialog.Show("Section View", "Please switch to level view.");
                            return Result.Cancelled;
                        }

                        var point = selection.PickPoint("Point to create a room");
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

                RoomDimensions roomDimensions = new RoomDimensions();
                grid.Scale = roomDimensions.GetScale(roomMin, roomMax, grid.CanvasSize);
                grid.RevitWalls = roomDimensions.GetWalls(newRoom);
                grid.Derrivation = GetDerrivation(box, grid);

                WpfCoordinates bBox = new WpfCoordinates();
                grid.BoundingBoxLines = bBox.GetBoundingBox(roomMin, roomMax, grid);

                SymbolPreselectCheck(window);
                grid.Draw();

                transaction.RollBack();
            }

            grid.TextBoxScale.Text = "Scale 1: " + grid.Scale;

            return Result.Succeeded;
        }

        private System.Drawing.Point GetDerrivation(BoundingBoxXYZ box, GridSetup grid)
        {
            System.Drawing.Point derrivationPoint = new System.Drawing.Point();

            var roomMin = new ConversionPoint(box.Min);
            var roomMax = new ConversionPoint(box.Max);

            double centerRoomX = roomMin.X / grid.Scale + (roomMax.X / grid.Scale - roomMin.X / grid.Scale) / 2;
            double centerRoomY = roomMin.Y / grid.Scale + (roomMax.Y / grid.Scale - roomMin.Y / grid.Scale) / 2;

            derrivationPoint.X = Convert.ToInt32(grid.CanvasSize / 2 - centerRoomX);
            derrivationPoint.Y = Convert.ToInt32(grid.CanvasSize / 2 + centerRoomY);

            return derrivationPoint;
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