using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Layouts;
#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace DCCPanelController.View;

[ObservableObject]
public partial class ControlPanelView {
    public enum CellHighlightAction {
        Selected,
        DragInvalid,
        DragValid,
        Resize
    }

    public event EventHandler<TileSelectedEventArgs>? TileSelected;

    private double _gridSize;
    private double _viewHeight;
    private double _viewWidth;
    private int _lastDragCol;
    private int _lastDragRow;
    private int _tapCount;

    private const int DoubleTapTime = 200;
    private bool _canDragTiles = true;

    public ControlPanelView() {
        InitializeComponent();
        BindingContext = this;
        SizeChanged += OnGridSizeChanged;
        MainGrid.SizeChanged += OnGridSizeChanged;
    }

    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;

    protected virtual void OnTileSelected(ITile? tile, int tapCount) => TileSelected?.Invoke(this, new TileSelectedEventArgs(tile, tapCount));

    private void OnGridSizeChanged(object? sender, EventArgs e) {
        DrawPanel();
    }

    public bool HasGridSizeChanged(double width, double height) {
        if (width < 1.0 || height < 1.0) return false;
        var roundWidth = ((int)(width * 100)) / 100;
        var roundHeight = ((int)(height * 100)) / 100;
        var difference = Math.Abs(CalculateGridSize(roundWidth, roundHeight) - _gridSize);
        return difference > 1;
    }

    public double CalculateGridSize(double width, double height) {
        if (width <= 0 || height <= 0) return 1;
        var gridSize = Math.Min(width / Cols, height / Rows);
        gridSize = Math.Floor(gridSize * 100) / 100.0;
        return gridSize;
    }

    public void ForceRefresh() {
        DrawPanel(true);
    }

    private void DrawPanel(bool forceRefresh = false) {
        // Only redraw the grid if we absolutely need to. Events may mean that this 
        // is called multiple times, but if we really have not changed, then do not 
        // waste time redrawing and rebuilding the grid. 
        // -------------------------------------------------------------------------
        if (Panel is null) return;
        if (MainGrid.Width < 1.0 || MainGrid.Height < 1.0) return;
        if (!forceRefresh && !HasGridSizeChanged(MainGrid.Width, MainGrid.Height)) return;

        using (new CodeTimer($"Draw Panel: {Panel.Title}/{Panel.Id}")) {
            _gridSize = CalculateGridSize(MainGrid.Width, MainGrid.Height);
            _viewWidth = _gridSize * Cols;
            _viewHeight = _gridSize * Rows;

            DynamicGrid.WidthRequest = _viewWidth;
            DynamicGrid.HeightRequest = _viewHeight;
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
        }
    }

    /// <summary>
    /// Given the Panel list of Entities, add each one as a tile to the panel.
    /// </summary>
    private void AddTilesToGrid(Panel? panel) {
        if (panel is null) return;
        foreach (var entity in panel.Entities) {
            AddTileToGrid(entity);
        }
    }

    /// <summary>
    /// Given an Entity, create a tile and add it to the panel grid. 
    /// </summary>
    /// <returns>Returns a instance of the created tile or null if it could not create one. </returns>
    private ITile? AddTileToGrid(Entity entity) {
        var tile = TileFactory.CreateTile(entity, _gridSize);
        if (tile is ContentView view) {
            // If this tile is an interactive tile, then add a guesture recogniser
            // so that when tapped or double-tapped, we can interact with it.
            // --------------------------------------------------------------------
            view.Behaviors.Clear();
            view.GestureRecognizers.Clear();

            var tapGesture = new TapGestureRecognizer() { NumberOfTapsRequired = 1 };
            tapGesture.Tapped += (_, args) => OnTileTapped(tile, args);
            view.GestureRecognizers.Add(tapGesture);

            if (DesignMode) {
                var touchBehavior = new CommunityToolkit.Maui.Behaviors.TouchBehavior();
                touchBehavior.LongPressCompleted += (_, args) => OnTileLongPressed(tile, args);
                view.Behaviors.Add(touchBehavior);

                var dragGesture = new DragGestureRecognizer();
                dragGesture.DragStarting += (_, args) => DragTileStarting(args, tile);
                dragGesture.DropCompleted += (sender, args) => DragCompleted(sender, args);
                view.GestureRecognizers.Add(dragGesture);
            }
            SetTileGridPosition(tile);
            DynamicGrid.Children.Add(view);
        }
        return tile;
    }
    
    private void OnTileLongPressed(object? sender, LongPressCompletedEventArgs e) {
        Console.WriteLine($"ControlPanelView.OnLongPressCompleted for {sender?.GetType()}");

        // TODO: Maybe this code draws the current path???
    }

    /// <summary>
    /// Handles the tile tap interaction, including single tap and optionally double-tap,
    /// to perform actions such as interaction, selection, rotation, or other context-specific operations.
    /// </summary>
    /// <param name="sender">
    /// The tile object that triggered the tap event. It can be an interactive or non-interactive tile.
    /// </param>
    /// <param name="e">
    /// The event arguments related to the tap event, providing details about the interaction.
    /// </param>
    private async void OnTileTapped(object? sender, TappedEventArgs e) {
        _tapCount++;
        await Task.Delay(DoubleTapTime);

        if (DesignMode) {
            if (sender is ITile tile) {
                if (_tapCount == 1) {
                    if (tile.IsSelected) {
                        ClearAllSelectedTiles();
                        OnTileSelected(null, 1);
                    } else {
                        ClearAllSelectedTiles();
                        MarkTileSelected(tile);
                        OnTileSelected(tile, 1);
                    }
                }
                if (_tapCount == 2) {
                    ClearAllSelectedTiles();
                    MarkTileSelected(tile);
                    OnTileSelected(tile, 2);
                }
            }
        } else {
            if (sender is ITileInteractive interactiveTile) {
                if (_tapCount == 1) interactiveTile.Interact();
                if (_tapCount == 2) interactiveTile.Secondary();
            }
        }
        _tapCount = 0;
    }

    private void RemoveTileFromGrid(ITile tile) => RemoveEntityFromGrid(tile.Entity);

    private void RemoveEntityFromGrid(Entity entity) {
        var children = DynamicGrid.Children.OfType<Microsoft.Maui.Controls.View>().Where(x => x.ClassId.Equals(entity.Guid.ToString())).ToList();
        foreach (var child in children) {
            DynamicGrid.Remove(child);
        }
    }

    private void SetTileGridPosition(ITile tile) {
        if (tile is ContentView view) {
            DynamicGrid.SetColumn(view, tile.Entity.Col);
            DynamicGrid.SetRow(view, tile.Entity.Row);
            DynamicGrid.SetColumnSpan(view, tile.Entity.Width);
            DynamicGrid.SetRowSpan(view, tile.Entity.Height);
        }
    }

    // If we click on a grid that is NOT a track piece and in design mode, 
    // then clear all the selected tracks.
    // -------------------------------------------------------------------------
    private void DynamicGridTapped(object? sender, TappedEventArgs e) {
        if (DesignMode) ClearAllSelectedTiles();
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
            AbsoluteLayout.SetLayoutBounds(graphicsView, new Rect(0.5, 0.5, _viewWidth, _viewHeight));
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

    #region Support Marking and UnMarking Tiles on the Panel
    public void MarkTileSelected(ITile tile) {
        if (tile is IView view) {
            var colSpan = DynamicGrid.GetColumnSpan(view);
            var rowSpan = DynamicGrid.GetRowSpan(view);
            var colPos = DynamicGrid.GetColumn(view);
            var rowPos = DynamicGrid.GetRow(view);
            Console.WriteLine($"Marking tile: {tile.Entity.Name} at {colPos} x {colSpan},{rowPos} x {rowSpan}");
        }
        
        HighlightCell(tile.Entity.Col, tile.Entity.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.Selected);
        tile.IsSelected = true;
        OnTileSelected(tile, 1);
    }

    public void MarkTileUnSelected(ITile tile) {
        UnHighlightCell(tile.Entity.Col, tile.Entity.Row);
        tile.IsSelected = false;
        OnTileSelected(null, 0);
    }

    public void ClearAllSelectedTiles() {
        var tiles = DynamicGrid.Children.OfType<ITile>().Where(x => x.IsSelected).ToList();
        foreach (var tile in tiles) tile.IsSelected = false;
        var children = DynamicGrid.Children.Where(x => x is Border border && x.Parent is Grid && border.ClassId == "CellHighlight").ToList();
        foreach (var child in children) DynamicGrid.Remove(child);
    }

    /// <summary>
    /// Only highlight a cell if we are in Design Mode
    /// </summary>
    public void HighlightCell(ITile tile, CellHighlightAction action) => HighlightCell(tile.Entity, action);

    public void HighlightCell(Entity entity, CellHighlightAction action) => HighlightCell(entity.Col, entity.Row, entity.Width, entity.Height, action);

    public void HighlightCell(int col, int row, int width, int height, CellHighlightAction action) {
        if (!DesignMode) return;
        
        UnHighlightCell(col, row);
        var color = action switch {
            CellHighlightAction.Selected    => Colors.CornflowerBlue,
            CellHighlightAction.Resize      => Colors.MidnightBlue,
            CellHighlightAction.DragValid   => Colors.Green,
            CellHighlightAction.DragInvalid => Colors.Red,
            _                               => Colors.Red
        };

        var borderColor = color;
        var backgroundColor = color.WithAlpha(0.25f);
        var border = new Border {
            ClassId = "CellHighlight",
            Stroke = borderColor,
            StrokeThickness = 2,
            BackgroundColor = backgroundColor,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            WidthRequest = width * _gridSize,
            HeightRequest = height * _gridSize,
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
    public void UnHighlightCell(ITile tile) => UnHighlightCell(tile.Entity);

    public void UnHighlightCell(Entity entity) => UnHighlightCell(entity.Col, entity.Row);

    public void UnHighlightCell(int col, int row) {
        if (!DesignMode) return;
        var children = DynamicGrid.Children.Where(x => x is Border border && x.Parent is Grid && border.ClassId == "CellHighlight").ToList();
        foreach (var child in children.Where(child => DynamicGrid.GetRow(child) == row && DynamicGrid.GetColumn(child) == col)) {
            DynamicGrid.Remove(child);
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
        // Some edit modes do not support drag and drop, and also if the mode is Resize and
        // the tile does not support resizing, then we can't allow it to resize. 
        // -------------------------------------------------------------------------------------------------
        if (_canDragTiles == false || (EditMode == EditModeEnum.Size && tile.Entity is not IDrawingEntity)) {
            args.Cancel = true;
            return;
        }

        args.Data.Properties.Add("Tile", tile);
        args.Data.Properties.Add("Source", "Panel");
        ClearAllSelectedTiles();
        _lastDragCol = EditMode == EditModeEnum.Size ? tile.Entity.Col : 0;
        _lastDragRow = EditMode == EditModeEnum.Size ? tile.Entity.Row : 0;

#if IOS || MACCATALYST
        UIDragPreview Action() {
            var image = EditMode switch {
                EditModeEnum.Copy => UIImage.FromFile("copy.png"),
                EditModeEnum.Move => UIImage.FromFile("move.png"),
                EditModeEnum.Size => UIImage.FromFile("crop.png"),
                _                 => UIImage.FromFile("move.png"),
            };
            var imageView = new UIImageView(image);
            imageView.ContentMode = UIViewContentMode.Center;
            imageView.Frame = new CGRect(0, 0, 32, 32);
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
        if (!DesignMode) {
#if IOS || MACCATALYST
            e?.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Forbidden));
#endif
            return;
        }

        var source = e.Data.Properties["Source"] as string ?? null;
        var tile = e.Data.Properties["Tile"] as ITile ?? null;
        var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));

        if (gridPosition is { } position && tile is not null) {
            UnHighlightCell(_lastDragCol, _lastDragRow);
            if (EditMode == EditModeEnum.Size && source == "Panel") {
                ResizeTrack(tile, position.Col, position.Row);
                HighlightCell(tile.Entity.Col, tile.Entity.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.Resize);
            } else {
                if (!DoesTrackClash(tile, position.Col, position.Row) && !IsTileOutOfBounds(tile, position.Col, position.Row)) {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragValid);
                } else {
                    e.AcceptedOperation = DataPackageOperation.None;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragInvalid);
                }
            }
            _lastDragCol = position.Col;
            _lastDragRow = position.Row;
        } else {
            UnHighlightCell(_lastDragCol, _lastDragRow);
            _lastDragCol = 0;
            _lastDragRow = 0;
        }

#if IOS || MACCATALYST
        if (source == "Symbol") {
            e?.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Copy));
        } else {
            e?.PlatformArgs?.SetDropProposal(EditMode switch {
                EditModeEnum.Copy => new UIDropProposal(UIDropOperation.Copy),
                EditModeEnum.Move => new UIDropProposal(UIDropOperation.Move),
                _                 => new UIDropProposal(UIDropOperation.Forbidden),
            });
        }
#endif

#if WINDOWS
        var dragUI = e.PlatformArgs.DragEventArgs.DragUIOverride;
        dragUI.IsCaptionVisible = false;
        dragUI.IsGlyphVisible = false;
#endif
    }

    private void DragCompleted(object? sender, DropCompletedEventArgs e) {
        if (sender is DragGestureRecognizer { Parent : ITile tile }) {
            MarkTileUnSelected(tile);
        }
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

            ClearAllSelectedTiles();
            var source = e.Data.Properties["Source"] as string ?? null;
            var tile = e.Data.Properties["Tile"] as ITile ?? null;
            var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));

            if (gridPosition is { } position && tile is not null) {
                // Make sure that the item we are placing is onto a point that is 
                // not already occupied unless the item being dropped is an overlay 
                // item that has a higher Z factor. 
                // -----------------------------------------------------------------
                if (!DoesTrackClash(tile, position.Col, position.Row)) {
                    if (Panel is { } panel) {
                        switch (source) {
                        case "Panel":
                            switch (EditMode) {
                            case EditModeEnum.Move:
                                tile.Entity.Col = position.Col;
                                tile.Entity.Row = position.Row;
                                SetTileGridPosition(tile);
                                MarkTileSelected(tile);
                                break;

                            case EditModeEnum.Copy:
                                var newEntity = panel.CreateEntityFrom(tile.Entity);
                                newEntity.Col = position.Col;
                                newEntity.Row = position.Row;
                                panel.AddEntity(newEntity);
                                break;

                            case EditModeEnum.Size:
                                ResizeTrack(tile, position.Col, position.Row);
                                MarkTileSelected(tile);
                                break;
                            }
                            break;

                        case "Symbol":
                            var dropEntity = panel.CreateEntityFrom(tile.Entity);
                            dropEntity.Col = position.Col;
                            dropEntity.Row = position.Row;
                            panel.AddEntity(dropEntity);
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

    private bool IsTileOutOfBounds(ITile tile, int positionCol, int positionRow) {
        return positionCol < 0 || positionCol >= Cols ||
               positionRow < 0 || positionRow >= Rows ||
               positionCol + tile.Entity.Width > Cols ||
               positionRow + tile.Entity.Height > Rows;
    }

    private bool DoesTrackClash(ITile tile, int col, int row) {
        if (tile.Entity is not ITrackEntity) return false;                                                  // No clashes possible if the track is not a track piece
        if (tile.Entity.Col == col && tile.Entity.Row == row && EditMode != EditModeEnum.Move) return true; // Can't drop onto yourself. 
        var tilesInGrid = DynamicGrid.OfType<ITile>()
                                     .Where(eTile =>

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

    private void ResizeTrack(ITile? tile, int newCol, int newRow) {
        if (tile is null) return;

        // Original position and size
        var originalX = tile.Entity.Col;
        var originalY = tile.Entity.Row;

        // Resizing right (increasing width)
        if (newCol > originalX) {
            tile.Entity.Width = newCol - originalX + 1; // +1 to include the new column
        }

        // Resizing left (shifting X and adjusting width)
        else if (newCol < originalX) {
            var deltaX = originalX - newCol;
            tile.Entity.Col -= deltaX;   // Shift left
            tile.Entity.Width += deltaX; // Increase width
        }

        // Resizing down (increasing height)
        if (newRow > originalY) {
            tile.Entity.Height = newRow - originalY + 1; // +1 to include the new row
        }

        // Resizing up (shifting Y and adjusting height)
        else if (newRow < originalY) {
            var deltaY = originalY - newRow;
            tile.Entity.Row -= deltaY;    // Shift up
            tile.Entity.Height += deltaY; // Increase height
        }

        // Ensure minimum size limits
        tile.Entity.Width = Math.Max(1, tile.Entity.Width);
        tile.Entity.Height = Math.Max(1, tile.Entity.Height);
        SetTileGridPosition(tile);
    }

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

public class TileSelectedEventArgs(ITile? tile, int tapCount) : EventArgs {
    public ITile? Tile { get; set; } = tile;
    public int TapCount { get; set; } = tapCount;
    public bool IsSingleTap => TapCount == 1;
    public bool IsDoubleTap => TapCount == 2;
}