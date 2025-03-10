using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Headers;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Layouts;
#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace DCCPanelController.View;

[ObservableObject]
public partial class ControlPanelView  {
    public enum CellHighlightAction {
        Selected,
        DragInvalid,
        DragValid
    }

    public ObservableCollection<ITile> Tiles = [];
    public Color GridColor = Colors.DarkGrey;
    public double GridSize;
    public double ViewHeight;
    public double ViewWidth;

    private int _lastDragCol;
    private int _lastDragRow;
    private int _tapCount = 0;
    private const int DoubleTapTime = 200;  

    public ControlPanelView() {
        InitializeComponent();
        BindingContext = this;
        MainGrid.SizeChanged += OnGridSizeChanged;
    }

    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;

    private void OnGridSizeChanged(object? sender, EventArgs e) {
        DrawPanel();
    }

    public bool HasGridSizeChanged(double width, double height) {
        if (width < 1.0 || height < 1.0) return false;
        var difference = Math.Abs(CalculateGridSize(width, height) - GridSize);
        return difference > 1;
    }

    public double CalculateGridSize(double width, double height) {
        if (width <= 0 || height <= 0) return 1;
        var gridSize = Math.Min(width / Cols, height / Rows);

        // Round down to the nearest 0.01
        gridSize = Math.Floor(gridSize * 100) / 100.0;
        return gridSize;
    }

    public void SetScreenSize(double width, double height) {
        GridSize = CalculateGridSize(width, height);
        ViewWidth = GridSize * Cols;
        ViewHeight = GridSize * Rows;
    }

    public void DrawPanel(bool forceRefresh = false) {
        // Only redraw the grid if we absolutely need to. Events may mean that this 
        // is called multiple times, but if we really have not changed, then do not 
        // waste time redrawing and rebuilding the grid. 
        // -------------------------------------------------------------------------
        if (Panel is null) return;
        if (MainGrid.Width < 1.0 || MainGrid.Height < 1.0) return;
        if (!forceRefresh && !HasGridSizeChanged(MainGrid.Width, MainGrid.Height)) return;

        var stopwatch = Stopwatch.StartNew();

        SetScreenSize(MainGrid.Width, MainGrid.Height);
        DynamicGrid.WidthRequest = ViewWidth;
        DynamicGrid.HeightRequest = ViewHeight;
        DynamicGrid.BackgroundColor = Panel?.BackgroundColor ?? Colors.Transparent;

        DynamicGrid.Children.Clear();

        if (DynamicGrid.RowDefinitions.Count != Rows || DynamicGrid.ColumnDefinitions.Count != Cols) {
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.ColumnDefinitions.Clear();

            for (var i = 0; i < Rows; i++) {
                DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            }

            for (var j = 0; j < Cols; j++) {
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }
        }

        DrawGrid();
        AddTilesToGrid(Panel);
        
        stopwatch.Stop();
        Console.WriteLine($"ControlPanelView.RebuildGrid: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Given the Panel list of Entities, add each one as a tile to the panel.
    /// </summary>
    private void AddTilesToGrid(Panel? panel) {
        if (panel is null) return;
        Tiles.Clear(); 
        foreach (var entity in panel.Entities) {
            AddTileToGrid(entity);    
        }
    }

    /// <summary>
    /// Given an Entity, create a tile and add it to the panel grid. 
    /// </summary>
    /// <returns>Returns a instance of the created tile or null if it could not create one. </returns>
    private ITile? AddTileToGrid(Entity entity) {
        var tile = TileFactory.CreateTile(entity, GridSize);
        if (tile is ContentView view) {
            // If this tile is an interactive tile, then add a guesture recogniser
            // so that when tapped, or double-tapped, we can interact with it.
            // --------------------------------------------------------------------
            view.Behaviors.Clear();
            view.GestureRecognizers.Clear();
            if (DesignMode) {
                var tapGesture = new TapGestureRecognizer() { NumberOfTapsRequired = 1 };
                tapGesture.Tapped += (_, args) => OnTileTappedInDesign(tile, args);
                view.GestureRecognizers.Add(tapGesture);

                var touchBehavior = new CommunityToolkit.Maui.Behaviors.TouchBehavior();
                touchBehavior.LongPressCompleted += (_, args) => OnTileLongPressed(tile, args);
                view.Behaviors.Add(touchBehavior);
                
                var dragGesture = new DragGestureRecognizer();
                dragGesture.DragStarting += (sender, args) => DragTileStarting(args, tile);
                view.GestureRecognizers.Add(dragGesture);

            } else {
                if (tile is ITileInteractive interactiveTile) {
                    var tapGesture = new TapGestureRecognizer() { NumberOfTapsRequired = 1 };
                    tapGesture.Tapped += (_, args) => OnTileTapped(tile, args);
                    view.GestureRecognizers.Add(tapGesture);
                }
            }

            DynamicGrid.SetColumn(view, tile.Entity.Col);
            DynamicGrid.SetRow(view, tile.Entity.Row);
            DynamicGrid.Children.Add(view);

        }
        return tile;
    }

    private void OnTileLongPressed(object? sender, LongPressCompletedEventArgs e) {
        Console.WriteLine($"ControlPanelView.OnLongPressCompleted for {sender?.GetType()}");
        // Add support for the Property Page to be shown
    }

    private async void OnTileTappedInDesign(object? sender, TappedEventArgs e) {
        _tapCount++;
        await Task.Delay(DoubleTapTime);
        Console.WriteLine($"ControlPanelView.OnTileTappedInDesign for {sender?.GetType()} with {_tapCount}");
        if (sender is ITile tile) {
            if (_tapCount == 1) ToggleMarkTile(tile);
            if (_tapCount == 2) tile.RotateRight();
        }
        _tapCount = 0;
    }
    
    private async void OnTileTapped(object? sender, TappedEventArgs e) {
        _tapCount++;
        await Task.Delay(DoubleTapTime);
        Console.WriteLine($"ControlPanelView.OnTileTapped for {sender?.GetType()} with {_tapCount}");
        if (sender is ITileInteractive interactiveTile) {
            if (_tapCount == 1) interactiveTile.Interact();
            if (_tapCount == 2) interactiveTile.Secondary();
        }
        _tapCount = 0;
    }
    
    private void RemoveTileFromGrid(ITile tile) {
        Panel?.Entities.Remove(tile.Entity);
        Tiles.Remove(tile);
    }

    private void SetTileGridPosition(ITile tile) {
        if (tile is ContentView view) {
            DynamicGrid.SetColumn(view, tile.Entity.Col);
            DynamicGrid.SetRow(view, tile.Entity.Row);
        }
    }
    
    // If we click on a grid that is NOT a track piece and in design mode, 
    // then clear all the selected tracks.
    // -------------------------------------------------------------------------
    private void DynamicGridTapped(object? sender, TappedEventArgs e) {
        Console.WriteLine("ControlPanelView.TapGestureRecognizer_OnTapped");
        if (DesignMode) ClearSelectedTiles();
    }

    #region Draw Grid when in Design Mode
    private void DrawGrid() {
        RemoveGrid();
        if (ShowGrid) {
            var gridLines = new GridLinesDrawable(Rows, Cols);
            var graphicsView = new GraphicsView {
                InputTransparent = true,
                Drawable = gridLines,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                ClassId = "GridLines"
            };

            // Add the GraphicsView directly to the AbsoluteLayout
            AbsoluteLayout.SetLayoutBounds(graphicsView, new Rect(0.5, 0.5, ViewWidth, ViewHeight));
            AbsoluteLayout.SetLayoutFlags(graphicsView, AbsoluteLayoutFlags.PositionProportional);
            ControlPanelLayout.Children.Add(graphicsView);
            graphicsView.Invalidate();
        }
    }
    
    /// <summary>
    ///     Draw the Grid Outline
    /// </summary>
    private void RemoveGrid() {
        if (ControlPanelLayout.Children.Count >= 1) {
            var graphicsViewToRemove = ControlPanelLayout.Children.OfType<GraphicsView>().ToList();
            foreach (var view in graphicsViewToRemove) {
                ControlPanelLayout.Children.Remove(view);
            }
        }
    }
    #endregion

    // public void RemoveTrackPiece(ITrack track) {
    //     if (Panel is { Tracks: { } tracks } panel) {
    //         if (track.Parent == panel) {
    //             track.Parent = null;
    //             MarkTrackUnSelected(track);
    //             RemoveDisplayItemFromGrid(track);
    //             Panel.Tracks.Remove(track);
    //         }
    //     }
    // }

    #region Support Marking and UnMarking Tiles on the Panel
    public void ToggleMarkTile(ITile tile) {
        if (tile.IsSelected) MarkTileUnSelected(tile); 
        else MarkTileSelected(tile);
    }

    public void MarkTileSelected(ITile tile) {
        HighlightCell(tile.Entity.Col, tile.Entity.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.Selected);
        tile.IsSelected = true;
    }
    
    public void MarkTileUnSelected(ITile tile) {
        UnHighlightCell(tile.Entity.Col, tile.Entity.Row);
        tile.IsSelected = false;
    }

    public void ClearSelectedTiles() {
        if (Panel is not null) {
            foreach (var tile in Tiles.Where(x => x.IsSelected)) MarkTileUnSelected(tile);
        }
    }
    
    /// <summary>
    ///     Only highlight a cell if we are in Design Mode
    /// </summary>
    public void HighlightCell(int col, int row, int width, int height, CellHighlightAction action) {
        if (!DesignMode) return;

        UnHighlightCell(col, row);
        var borderColor = action switch {
            CellHighlightAction.Selected    => Colors.CornflowerBlue,
            CellHighlightAction.DragValid   => Colors.Green,
            CellHighlightAction.DragInvalid => Colors.Red,
            _                               => Colors.Red
        };

        var backgroundColor = action switch {
            CellHighlightAction.Selected    => Colors.CornflowerBlue.WithAlpha(0.25f),
            CellHighlightAction.DragValid   => Colors.Transparent,
            CellHighlightAction.DragInvalid => Colors.Transparent,
            _                               => Colors.Transparent
        };

        var border = new Border {
            ClassId = "CellHighlight",
            Stroke = borderColor,
            StrokeThickness = 3,
            BackgroundColor = backgroundColor.WithAlpha(0.25f),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            WidthRequest = width * GridSize,
            HeightRequest = height * GridSize,
            ZIndex = EntityPresets.Highlight,
            InputTransparent = true
        };

        // Add the Track DisplayImage to the appropriate grid position
        // ------------------------------------------------------
        DynamicGrid.SetRow(border, row);
        DynamicGrid.SetColumn(border, col);
        DynamicGrid.Children.Add(border);
    }

    /// <summary>
    ///     Only UnHighlight a cell if we are operating in Design mode
    ///     If we are in Operate mode, then we do not highlight cells so this has no function.
    /// </summary>
    public void UnHighlightCell(int col, int row) {
        if (!DesignMode) return;
        var children = DynamicGrid.Children.Where(x => x is Border border && x.Parent is Grid && border.ClassId == "CellHighlight").ToList();
        foreach (var child in children) {
            if (DynamicGrid.GetRow(child) == row && DynamicGrid.GetColumn(child) == col) {
                DynamicGrid.Remove(child);
            }
        }
    }
    #endregion
    
    #region Drag and Drop Support for the Tiles
    
    /// <summary>
    /// If we have started a Drag and it is a Tile then store the tile we are dragging in the drag properties
    /// and also that the source is a Tile. We use this along with the mode to either move the tile or add
    /// a new tile to the panel. 
    /// </summary>
    private void DragTileStarting(DragStartingEventArgs args, ITile tile) {
        args.Data.Properties.Add("Tile", tile);
        args.Data.Properties.Add("Source", "Panel");
        _lastDragCol = 0;
        _lastDragRow = 0;

        #if IOS || MACCATALYST
        UIDragPreview Action() {
            var image = EditMode switch {
                EditModeEnum.Copy => UIImage.FromFile("copy.png"),
                EditModeEnum.Move => UIImage.FromFile("move.png"),
                EditModeEnum.Size => UIImage.FromFile("crop.png"),
                _ => UIImage.FromFile("move.png"),
            };
            var imageView = new UIImageView(image);
            imageView.ContentMode = UIViewContentMode.Center;
            imageView.Frame = new CGRect(0, 0, 10, 10);
            return new UIDragPreview(imageView);
        }
        args?.PlatformArgs?.SetPreviewProvider(Action);
        #endif
    }

    /// <summary>
    /// Called when we have left the bounds of thr Panel so we just reset everything
    /// </summary>
    private void DragLeaveTileOnPanel(object? sender, DragEventArgs e) {
        UnHighlightCell(_lastDragCol, _lastDragRow);
        _lastDragCol = 0;
        _lastDragRow = 0;
    }

    /// <summary>
    /// Called when we are dragging a tile on the panel surface. Works out if it is a valid drop zone
    /// or if it would clash with something else. For example, you cannot have a track on another track but
    /// you could have a non-track on top of a track or a track on top of an image. 
    /// </summary>
    private void DragOverTileOnPanel(object? sender, DragEventArgs e) {
        var tile = e.Data.Properties["Tile"] as ITile ?? null;
        var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));
        
        if (gridPosition is {} position && tile is not null) { 
            if (EditMode == EditModeEnum.Size) {
                //ResizeTrack(track, position.Col, position.Row);
            } else {
                if (_lastDragCol != position.Col || _lastDragRow != position.Row) {
                    UnHighlightCell(_lastDragCol, _lastDragRow);
                }

                if (!DoesTrackClash(tile, position.Col, position.Row)) {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragValid);
                } else {
                    e.AcceptedOperation = DataPackageOperation.None;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragInvalid);
                }
                _lastDragCol = position.Col;
                _lastDragRow = position.Row;
            }
        } else {
            UnHighlightCell(_lastDragCol, _lastDragRow);
            _lastDragCol = 0;
            _lastDragRow = 0;
        }

        #if IOS || MACCATALYST
        e?.PlatformArgs?.SetDropProposal(EditMode switch {
            EditModeEnum.Copy => new UIDropProposal(UIDropOperation.Copy),
            EditModeEnum.Move => new UIDropProposal(UIDropOperation.Move),
            _                 => new UIDropProposal(UIDropOperation.Forbidden),
            });
        #endif

        #if WINDOWS
        var dragUI = e.PlatformArgs.DragEventArgs.DragUIOverride;
        dragUI.IsCaptionVisible = false;
        dragUI.IsGlyphVisible = false;
        #endif
    }

    /// <summary>
    /// Support dropping the dragged tile onto the panel in a new position (or the same position) 
    /// </summary>
    private void DropTileOnPanel(object? sender, DropEventArgs e) {
        try {
            if (!e.Data.Properties.ContainsKey("Source") ||
                !e.Data.Properties.ContainsKey("Tile")) {
                Console.WriteLine("Invalid Drop Properties.");
                return;
            }

            UnHighlightCell(_lastDragCol, _lastDragRow);
            var source = e.Data.Properties["Source"] as string ?? null;
            var tile = e.Data.Properties["Tile"] as ITile ?? null;
            var gridPosition = GetGridPosition(e?.GetPosition(DynamicGrid));

            if (gridPosition is { } position && tile is not null) {
                // Make sure that the item we are placing is onto a point that is 
                // not already occupied unless the item being dropped is an overlay 
                // item that has a higher Z factor. 
                // -----------------------------------------------------------------
                if (!DoesTrackClash(tile, position.Col, position.Row)) {
                    ClearSelectedTiles();

                    if (Panel is { } panel) {
                        switch (source) {
                        case "Panel":
                            switch (EditMode) {
                            case EditModeEnum.Move:
                                tile.Entity.Col = position.Col;
                                tile.Entity.Row = position.Row;
                                SetTileGridPosition(tile);
                                break;

                            case EditModeEnum.Copy:
                                var newEntity = panel.CreateEntityFrom(tile.Entity);
                                newEntity.Col = position.Col;
                                newEntity.Row = position.Row;
                                var newTile = AddTileToGrid(newEntity);
                                if (newTile is not null) MarkTileSelected(newTile);
                                break;

                            case EditModeEnum.Size:
                                break;
                            }
                            break;

                        case "DisplaySymbol":
                            //if (Panel is not null && trackPiece.Clone(Panel) is { } newPiece) {
                            //    newPiece.X = position.Col;
                            //    newPiece.Y = position.Row;
                            //    Panel?.AddTrack(newPiece);
                            //    AddDisplayItemToGrid(newPiece);
                            //    MarkTileSelected(newPiece);
                            //}

                            break;

                        default:
                            Debug.WriteLine($"ERROR: Invalid source: '{source}'");
                            break;
                        }
                    } 
                } else {
                    Debug.WriteLine("ERROR: Item clashes with existing track");
                }
            } else {
                Debug.WriteLine("ERROR: Invalid grid position");
            }
        } catch (Exception ex) {
            Debug.WriteLine("ERROR: Error dropping item: " + ex.Message);
        }

        _lastDragCol = 0;
        _lastDragRow = 0;
    }

    private bool DoesTrackClash(ITile tile, int col, int row) {
        if (Tiles.Count == 0) return false;                 // No clashes possible if no tiles are present
        if (tile.Entity is not ITrackEntity) return false;  // No clashes possible if the track is not a track piece
        
        var tilesInGrid = Tiles.Where(eTile =>              
                // Exclude the same track we're checking against
                eTile != tile && eTile.Entity is ITrackEntity &&

                // Check if there's a column overlap between the tracks
                col < eTile.Entity.Col + eTile.Entity.Width && col + eTile.Entity.Width > eTile.Entity.Col &&

                // Check if there's a row overlap between the tracks
                row < eTile.Entity.Row + eTile.Entity.Height && row + eTile.Entity.Height > eTile.Entity.Row
        );

        // If there are any tracks in the clashing list, return true
        return tilesInGrid.Any();
    }
    #endregion
    
//
//     private void ResizeTrack(ITrack? track, int newCol, int newRow) {
//         if (track is null) return;
//
//         // Original position and size
//         var originalX = track.X;
//         var originalY = track.Y;
//         var originalWidth = track.Width;
//         var originalHeight = track.Height;
//
//         // Resizing right (increasing width)
//         if (newCol > originalX) {
//             track.Width = newCol - originalX + 1; // +1 to include the new column
//         }
//
//         // Resizing left (shifting X and adjusting width)
//         else if (newCol < originalX) {
//             var deltaX = originalX - newCol;
//             track.X -= deltaX;     // Shift left
//             track.Width += deltaX; // Increase width
//         }
//
//         // Resizing down (increasing height)
//         if (newRow > originalY) {
//             track.Height = newRow - originalY + 1; // +1 to include the new row
//         }
//
//         // Resizing up (shifting Y and adjusting height)
//         else if (newRow < originalY) {
//             var deltaY = originalY - newRow;
//             track.Y -= deltaY;      // Shift up
//             track.Height += deltaY; // Increase height
//         }
//
//         // Ensure minimum size limits
//         track.Width = Math.Max(1, track.Width);
//         track.Height = Math.Max(1, track.Height);
//
//         // Refresh Grid and Update
//         InvalidateCell(track); // Re-render the grid for the resized component
//     }

    /// <summary>
    ///     Convert a position in the grid (absolute) to a Grid position within the col/row definitions
    /// </summary>
    /// <param name="point">A point object of where the item was tapped</param>
    /// <returns>Either a null, or (-1,-1) or (row,col) </returns>
    private (int Col, int Row)? GetGridPosition(Point? point) {
        if (point is { } tapPosition) {
            var totalHeight = DynamicGrid.Height;
            var totalWidth = DynamicGrid.Width;
            var rowCount = DynamicGrid.RowDefinitions.Count;
            var colCount = DynamicGrid.ColumnDefinitions.Count;

            var cellHeight = totalHeight / rowCount;
            var cellWidth = totalWidth / colCount;

            if (cellHeight == 0 || cellWidth == 0) {
                Console.WriteLine($"Cell Height or Width is zero? {cellHeight},{cellWidth}");
                return null;
            }

            // Calculate row and column indices
            var row = (int)(tapPosition.Y / cellHeight);
            var col = (int)(tapPosition.X / cellWidth);

            // Ensure indices are within bounds
            row = Math.Min(row, rowCount - 1);
            col = Math.Min(col, colCount - 1);

            return (col, row);
        }
        Console.WriteLine($"Could not determine the Grid Position from the point provided: {point.ToString()}");
        return null;
    }
}

public enum EditModeEnum {
    Move,
    Copy,
    Size
}