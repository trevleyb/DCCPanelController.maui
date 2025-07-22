using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.PathFinder;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Layouts;
#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace DCCPanelController.View;

[ObservableObject]
public partial class ControlPanelView {
    private readonly ILogger<ControlPanelView> _logger;

    public enum CellHighlightAction {
        Selected,
        DragInvalid,
        DragValid,
        Resize
    }

    private readonly PathTracingService _pathTracer = new();

    private const int DoubleTapTime = 150;
    private readonly bool _canDragTiles = true;
    private readonly HashSet<ITile> _selectedTiles = [];

    private int _dragStartCol;
    private int _dragStartRow;
    private int _lastDragCol;
    private int _lastDragRow;
    private int _tapCount;
    private int _currentSelectionIndex;

    private double _gridSize;
    private double _viewHeight;
    private double _viewWidth;

    public ControlPanelView() {
        _logger = MauiProgram.ServiceHelper.GetService<ILogger<ControlPanelView>>();
        InitializeComponent();
        BindingContext = this;
        SizeChanged += OnGridSizeChanged;
        MainGrid.SizeChanged += OnGridSizeChanged;
    }

    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;

    private string GetEditModeIconFilename =>
        EditMode switch {
            EditModeEnum.Copy => "copy.png",
            EditModeEnum.Move => "move.png",
            EditModeEnum.Size => "crop.png",
            _                 => "move.png"
        };

    #region Event Handlers
    public event EventHandler<TileSelectedEventArgs>? TileChanged;
    public event EventHandler<TileSelectedEventArgs>? TileSelected;
    public event EventHandler<TileSelectedEventArgs>? TileTapped;

    private void OnTileChanged(ITile tile) {
        TileChanged?.Invoke(this, new TileTappedEventArgs(tile, 0));
    }

    private void OnTileTapped(ITile tile, int tapCount) {
        TileTapped?.Invoke(this, new TileTappedEventArgs(tile, tapCount));
    }

    private void OnTileSelected(int tapCount) {
        TileSelected?.Invoke(this, new TileSelectedEventArgs(_selectedTiles, tapCount));
    }
    #endregion

    #region Grid Management
    private async void OnGridSizeChanged(object? sender, EventArgs e) {
        try {
            await DrawPanel();
        } catch (Exception ex) {
            _logger.LogError("OnGridSizeChanged Threw Error: {Message}", ex.Message);
        }
    }

    public bool HasGridSizeChanged(double width, double height) {
        if (width < 1.0 || height < 1.0) return false;

        // Use a small epsilon for floating-point comparison. This is more
        // robust and will detect smaller, but still significant, size changes.
        const double Epsilon = 0.01;

        var newGridSize = CalculateGridSize(width, height);
        var difference = Math.Abs(newGridSize - _gridSize);

        return difference > Epsilon;

        // if (width < 1.0 || height < 1.0) return false;
        // var roundWidth = (int)(width * 100) / 100;
        // var roundHeight = (int)(height * 100) / 100;
        // var difference = Math.Abs(CalculateGridSize(roundWidth, roundHeight) - _gridSize);
        // return difference > 1;
    }

    public double CalculateGridSize(double width, double height) {
        if (width <= 0 || height <= 0) return 1;
        var gridSize = Math.Min(width / Cols, height / Rows);
        gridSize = Math.Floor(gridSize * 100) / 100.0;
        return gridSize;
    }

    public async Task ForceRefresh() {
        await DrawPanel(true);
    }

    private async Task DrawPanel(bool forceRefresh = false,
                           [CallerMemberName] string memberName = "",
                           [CallerFilePath] string sourceFilePath = "",
                           [CallerLineNumber] int sourceLineNumber = 0) {

        if (Panel is null) return;
        if (Panel.IsRefreshing) return;
        if (MainGrid.Width < 1.0 || MainGrid.Height < 1.0) return;
        if (!forceRefresh && !HasGridSizeChanged(MainGrid.Width, MainGrid.Height)) return;

        // Only redraw the grid if we absolutely need to. Events may mean that this 
        // is called multiple times, but if we really have not changed, then do not 
        // waste time redrawing and rebuilding the grid. 
        // -------------------------------------------------------------------------
        _logger.LogDebug("DrawPanel: From='{MemberName}' @ '{SourceLineNumber}' Panel='{Panel}' Forced='{Forced}' Changed='{HasChanged}' ", memberName, sourceLineNumber, Panel?.Id ?? "UNKNOWN PANEL???", forceRefresh, HasGridSizeChanged(MainGrid.Width, MainGrid.Height));
        try {
            Panel!.IsRefreshing = true;
            await Task.Delay(10);    // Yield to allow UI to update
            
            ClearAllSelectedTiles();
            using (new CodeTimer($"Draw Panel: {Panel?.Id} called from {memberName}@{sourceLineNumber}", true)) {
                _gridSize = CalculateGridSize(MainGrid.Width, MainGrid.Height);
                _viewWidth = _gridSize * Cols;
                _viewHeight = _gridSize * Rows;

                DynamicGrid.ZIndex = 0;
                DynamicGrid.WidthRequest = _viewWidth;
                DynamicGrid.HeightRequest = _viewHeight;
                DynamicGrid.BackgroundColor = Panel?.PanelBackgroundColor ?? Colors.Transparent;

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
                AddEntitiesToGrid(Panel);
                DrawGrid();
            }
        } finally {
            Panel!.IsRefreshing = false;
        }
    }

    /// <summary>
    ///     Given the Panel list of Entities, add each one as a tile to the panel.
    /// </summary>
    private void AddEntitiesToGrid(Panel? panel) {
        if (panel is null) return;
        _pathTracer.ClearTileRegistry();
        foreach (var entity in panel.Entities.OrderBy(x => x.Layer)) {
            AddEntityToGrid(entity);
        }
    }

    /// <summary>
    ///     Given an Entity, create a tile and add it to the panel grid.
    /// </summary>
    /// <returns>Returns an instance of the created tile or null if it could not create one. </returns>
    private ITile? AddEntityToGrid(Entity entity) {
        using (new CodeTimer($"Add Entity to Grid: {entity.EntityName}:{entity.Guid} @ {entity.Col},{entity.Row}", false)) {
            var tile = TileFactory.CreateTile(entity, _gridSize, DesignMode ? TileDisplayMode.Design : TileDisplayMode.Normal);
            if (tile is ContentView view) {
                view.ClassId = entity.Guid.ToString();
                SetTileGestures(tile);
                SetTileGridPosition(tile);
                DynamicGrid.Children.Add(view);
                if (tile is TrackTile trackTile) _pathTracer.RegisterTile(trackTile);
            }
            return tile;
        }
    }

    private void SetTileGestures(ITile tile) {
        if (tile is ContentView view) {
            // If this tile is an interactive tile, then add a guesture recogniser
            // so that when tapped or double-tapped, we can interact with it.
            // --------------------------------------------------------------------
            view.Behaviors.Clear();
            view.GestureRecognizers.Clear();

            if (Interactive || DesignMode) {
                var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
                tapGesture.Tapped += (_, args) => OnTileTapped(tile, args);
                view.GestureRecognizers.Add(tapGesture);
            }

            switch (DesignMode) {
            case false: {
                if (Interactive) {
                    var touchBehavior = new TouchBehavior();
                    touchBehavior.LongPressCompleted += (_, args) => OnTileLongPressed(tile, args);
                    view.Behaviors.Add(touchBehavior);
                }
                break;
            }

            case true: {
                var dragGesture = new DragGestureRecognizer();
                dragGesture.DragStarting += (_, args) => DragTileStarting(args, tile);
                dragGesture.DropCompleted += DragCompleted;
                view.GestureRecognizers.Add(dragGesture);
                break;
            }
            }
        }
    }

    private async void OnTileLongPressed(object? sender, LongPressCompletedEventArgs e) {
        try {
            if (DesignMode) {
                OnTileSelected(-1);
            } else if (sender is TrackTile trackTile) {
                await _pathTracer.StartPathTracing(trackTile!);
            }
        } catch (Exception ex) {
            _logger.LogError($"Error in launching path Tracing: {ex.Message}");
        }
    }

    /// <summary>
    ///     Handles the tile tap interaction, including single tap and optionally double-tap,
    ///     to perform actions such as interaction, selection, rotation, or other context-specific operations.
    /// </summary>
    /// <param name="sender">
    ///     The tile object that triggered the tap event. It can be an interactive or non-interactive tile.
    /// </param>
    /// <param name="e">
    ///     The event arguments related to the tap event, providing details about the interaction.
    /// </param>
    private async void OnTileTapped(object? sender, TappedEventArgs e) {
        try {
            _tapCount++;
            await Task.Delay(DoubleTapTime);

            if (DesignMode) {
                if (sender is ITile tile) {
                    if (_tapCount == 1) {
                        var tilesAtPosition = TilesInGrid(tile);

                        // If there is only a single tile at this position, then we can 
                        // toggle it on or off
                        // -------------------------------------------------------
                        if (tilesAtPosition.Count is 0 or 1) {
                            if (tile.IsSelected) {
                                MarkTileUnSelected(tile);
                            } else {
                                MarkTileSelected(tile);
                            }
                            _currentSelectionIndex = -1;
                        }

                        // There are multiple tiles at this position, so we need to
                        // step through the tiles until we get to the last one. 
                        // ---------------------------------------------------------
                        else {
                            _currentSelectionIndex++;
                            foreach (var posTile in tilesAtPosition) MarkTileUnSelected(posTile);
                            if (_currentSelectionIndex >= tilesAtPosition.Count) {
                                _currentSelectionIndex = -1;
                            } else {
                                var selectedTile = tilesAtPosition[_currentSelectionIndex];
                                MarkTileSelected(selectedTile);
                            }
                        }
                        OnTileSelected(_tapCount);
                    }
                    if (_tapCount == 2) {
                        ClearAllSelectedTiles();
                        MarkTileSelected(tile);
                        OnTileSelected(_tapCount);
                    }
                }
            } else {
                if (sender is ITile tile) OnTileTapped(tile, _tapCount);
            }
            _tapCount = 0;
        } catch (Exception ex) {
            _logger.LogDebug("Tap Tile failed due to: {ExMessage}", ex.Message);
        }
    }

    private List<Tile> TilesInGrid(ITile tile) {
        var children = DynamicGrid.Children.OfType<Tile>()
                                  .Where(x => x.Entity.Col == tile.Entity.Col && x.Entity.Row == tile.Entity.Row)
                                  .OrderByDescending(x => x.Entity.Layer) // Highest layer first for selection
                                  .ToList();

        return children; // Add the missing return statement
    }

    private void RemoveTileFromGrid(ITile tile) {
        RemoveEntityFromGrid(tile.Entity);
        OnTileChanged(tile);
    }

    private void RemoveEntityFromGrid(Entity entity) {
        var children = DynamicGrid.Children.OfType<Microsoft.Maui.Controls.View>().Where(x => x.ClassId != null && x.ClassId.Equals(entity.Guid.ToString())).ToList();
        foreach (var child in children) {
            child.GestureRecognizers.Clear();
            DynamicGrid.Remove(child);
            if (child is TrackTile trackTile) _pathTracer.UnregisterTile(trackTile);
            var tiles = DynamicGrid.Children.OfType<ITile>().Where(x => x.Entity.Guid == entity.Guid).ToList();
            foreach (var tile in tiles) MarkTileUnSelected(tile);
        }
    }

    // If we click on a grid that is NOT a track piece and in design mode, 
    // then clear all the selected tracks.
    // -------------------------------------------------------------------------
    private void DynamicGridTapped(object? sender, TappedEventArgs e) {
        if (DesignMode) ClearAllSelectedTiles();
    }

    private void ResizeTrack(ITile? tile, int newCol, int newRow) {
        if (tile is null) return; // Only resize tracks

        // Current size and position of the tile during drag
        var currentCol = _dragStartCol; // Always work from the starting position
        var currentRow = _dragStartRow;

        // Work relative to the drag start position
        if (newCol >= _dragStartCol) {
            // Dragging right - increase width and keep column the same
            tile.Entity.Col = _dragStartCol;
            tile.Entity.Width = newCol - _dragStartCol + 1;
        } else {
            // Dragging left - move column left and increase width
            tile.Entity.Col = newCol;
            tile.Entity.Width = _dragStartCol - newCol + 1;
        }

        if (newRow >= _dragStartRow) {
            // Dragging down - increase height and keep row the same
            tile.Entity.Row = _dragStartRow;
            tile.Entity.Height = newRow - _dragStartRow + 1;
        } else {
            // Dragging up - move row up and increase height
            tile.Entity.Row = newRow;
            tile.Entity.Height = _dragStartRow - newRow + 1;
        }

        // Ensure minimum size limits (width and height shouldn't be less than 1)
        tile.Entity.Width = Math.Max(1, tile.Entity.Width);
        tile.Entity.Height = Math.Max(1, tile.Entity.Height);

        // Update the UI Grid to reflect the changes
        SetTileGridPosition(tile);
        tile.ForceRedraw();
        OnTileChanged(tile);
    }

    private void SetTileGridPosition(ITile tile) {
        if (tile is ContentView view) {
            DynamicGrid.SetColumn(view, tile.Entity.Col);
            DynamicGrid.SetRow(view, tile.Entity.Row);
            DynamicGrid.SetColumnSpan(view, tile.Entity.Width);
            DynamicGrid.SetRowSpan(view, tile.Entity.Height);
        }
    }
    #endregion

    #region Position Helpers
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
                _logger.LogDebug("Cell Height or Width is zero? {CellHeight},{CellWidth}", cellHeight, cellWidth);
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
        _logger.LogError("Could not determine the Grid Position from the point provided: {S}", point.ToString());
        return null;
    }
    #endregion

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
        _selectedTiles.Add(tile);
        HighlightCell(tile.Entity.Col, tile.Entity.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.Selected);
        tile.IsSelected = true;

        //OnTileSelected(tile, 1);
    }

    public void MarkTileUnSelected(ITile tile) {
        _selectedTiles.Remove(tile);
        UnHighlightCell(tile.Entity.Col, tile.Entity.Row);
        tile.IsSelected = false;
    }

    public void ClearAllSelectedTiles() {
        // Remove each tile from the collection of selected tiles.
        // -------------------------------------------------------
        foreach (var tile in _selectedTiles) {
            MarkTileUnSelected(tile);
        }

        // Do a clean-up in case anything is left over? There should not be anything.
        // -------------------------------------------------------
        var children = DynamicGrid.Children.Where(x => x is Border border && x.Parent is Grid && border.ClassId == "CellHighlight").ToList();
        foreach (var child in children) DynamicGrid.Remove(child);

        _selectedTiles.Clear();
        OnTileSelected(0);

        // var tiles = DynamicGrid.Children.OfType<ITile>().Where(x => x.IsSelected).ToList();
        // foreach (var tile in tiles) MarkTileUnSelected(tile);
    }

    /// <summary>
    ///     Only highlight a cell if we are in Design Mode
    /// </summary>
    public void HighlightCell(ITile tile, CellHighlightAction action) {
        HighlightCell(tile.Entity, action);
    }

    public void HighlightCell(Entity entity, CellHighlightAction action) {
        HighlightCell(entity.Col, entity.Row, entity.Width, entity.Height, action);
    }

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
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
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
    public void UnHighlightCell(ITile tile) {
        UnHighlightCell(tile.Entity);
    }

    public void UnHighlightCell(Entity entity) {
        UnHighlightCell(entity.Col, entity.Row);
    }

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
    ///     If we have started a Drag and it is a Tile then store the tile we are dragging in the drag properties
    ///     and also that the source is a Tile. We use this along with the mode to either move the tile or add
    ///     a new tile to the panel.
    /// </summary>
    private void DragTileStarting(DragStartingEventArgs args, ITile tile) {
        // Some edit modes do not support drag and drop, and also if the mode is Resize and
        // the tile does not support resizing, then we can't allow it to resize. 
        // -------------------------------------------------------------------------------------------------
        if (_canDragTiles == false || EditMode == EditModeEnum.Size && tile.Entity is not IDrawingEntity) {
            args.Cancel = true;
            return;
        }

        args.Data.Properties.Add("Tile", tile);
        args.Data.Properties.Add("Source", "Panel");
        args.Data.Image = GetEditModeIconFilename;

        ClearAllSelectedTiles();
        _lastDragCol = tile.Entity.Col;
        _lastDragRow = tile.Entity.Row;
        _dragStartCol = tile.Entity.Col;
        _dragStartRow = tile.Entity.Row;

#if IOS || MACCATALYST
        UIDragPreview Action() {
            var image = UIImage.FromFile(GetEditModeIconFilename);
            var imageView = new UIImageView(image);
            imageView.ContentMode = UIViewContentMode.Center;
            imageView.Frame = new CGRect(0, 0, 32, 32);
            return new UIDragPreview(imageView);
        }

        args?.PlatformArgs?.SetPreviewProvider(Action);
#endif
    }

    /// <summary>
    ///     Called when we have left the bounds of thr Panel so we just reset everything
    /// </summary>
    private void DragLeaveTileOnPanel(object? sender, DragEventArgs e) {
        UnHighlightCell(_lastDragCol, _lastDragRow);
        _lastDragCol = 0;
        _lastDragRow = 0;
    }

    /// <summary>
    ///     Called when we are dragging a tile on the panel surface. Works out if it is a valid drop zone
    ///     or if it would clash with something else. For example, you cannot have a track on another track but
    ///     you could have a non-track on top of a track or a track on top of an image.
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

        if (gridPosition is { } position && tile is not null && (position.Col != _lastDragCol || position.Row != _lastDragRow)) {
            if (EditMode == EditModeEnum.Size && source == "Panel") {
                UnHighlightCell(_lastDragCol, _lastDragRow);
                ResizeTrack(tile, position.Col, position.Row);
                HighlightCell(tile, CellHighlightAction.Resize);
                _lastDragCol = tile.Entity.Col;
                _lastDragRow = tile.Entity.Row;
            } else {
                UnHighlightCell(_lastDragCol, _lastDragRow);
                if (!DoesTrackClash(tile, position.Col, position.Row) && !IsTileOutOfBounds(tile, position.Col, position.Row)) {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragValid);
                } else {
                    e.AcceptedOperation = DataPackageOperation.None;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragInvalid);
                }
                _lastDragCol = position.Col;
                _lastDragRow = position.Row;
            }
        }

#if IOS || MACCATALYST
        if (source == "Symbol") {
            e?.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Copy));
        } else {
            e?.PlatformArgs?.SetDropProposal(EditMode switch {
                EditModeEnum.Copy => new UIDropProposal(UIDropOperation.Copy),
                EditModeEnum.Move => new UIDropProposal(UIDropOperation.Move),
                EditModeEnum.Size => new UIDropProposal(UIDropOperation.Move),
                _                 => new UIDropProposal(UIDropOperation.Forbidden)
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
        var complete = (sender is DragGestureRecognizer { Parent : ITile });
        if (sender is DragGestureRecognizer { Parent : ITile tile }) {
            tile.ForceRedraw();
            ClearAllSelectedTiles();
        }
    }

    /// <summary>
    ///     Support dropping the dragged tile onto the panel in a new position (or the same position)
    /// </summary>
    private void DropTileOnPanel(object? sender, DropEventArgs e) {
        _logger.LogInformation("DropTileOnPanel Called.");
        try {
            if (!e.Data.Properties.ContainsKey("Source") ||
                !e.Data.Properties.ContainsKey("Tile")) {
                _logger.LogInformation("DropTileOnPanel Called: No source or tile");
                return;
            }

            ClearAllSelectedTiles();
            var source = e.Data.Properties["Source"] as string ?? null;
            var tile = e.Data.Properties["Tile"] as ITile ?? null;
            var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));

            if (gridPosition is { } position && tile is not null) {
                _logger.LogInformation("DropTileOnPanel Source='{Source}' Tile='{Tile}' Position='{Col},{Row}'", source, tile.Entity.EntityName ?? "Undefined", position.Col, position.Row);

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
                                _logger.LogInformation("DropTileOnPanel: Mode=Move");
                                tile.Entity.Col = position.Col;
                                tile.Entity.Row = position.Row;
                                SetTileGridPosition(tile);
                                MarkTileSelected(tile);
                                OnTileChanged(tile);
                                break;

                            case EditModeEnum.Copy:
                                _logger.LogInformation("DropTileOnPanel: Mode=Copy");
                                var newEntity = panel.CreateEntityFrom(tile.Entity);
                                newEntity.Col = position.Col;
                                newEntity.Row = position.Row;
                                panel.AddEntity(newEntity);
                                OnTileChanged(tile);
                                break;

                            case EditModeEnum.Size:
                                _logger.LogInformation("DropTileOnPanel: Mode=Size");
                                ResizeTrack(tile, position.Col, position.Row);
                                MarkTileSelected(tile);
                                OnTileChanged(tile);
                                break;

                            default:
                                _logger.LogError("ERROR: Invalid operation?");
                                break;
                            }
                            break;

                        case "Symbol":
                            _logger.LogInformation("DropTileOnPanel: Mode=Symbol");
                            var dropEntity = panel.CreateEntityFrom(tile.Entity);
                            dropEntity.Col = position.Col;
                            dropEntity.Row = position.Row;
                            panel.AddEntity(dropEntity);
                            ClearAllSelectedTiles();
                            OnTileChanged(tile);
                            break;

                        default:
                            _logger.LogError("ERROR: Invalid source: '{Source}'", source);
                            break;
                        }
                    }
                } else {
                    _logger.LogError("ERROR: Item clashes with existing track");
                }
            } else {
                _logger.LogError("ERROR: Invalid grid position");
            }
        } catch (Exception ex) {
            _logger.LogError("ERROR: Error dropping item: {ExMessage} ", ex.Message);
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

    #region Bindable Properties
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty InteractiveProperty = BindableProperty.Create(nameof(Interactive), typeof(bool), typeof(ControlPanelView), true, BindingMode.Default, propertyChanged: OnInteractiveChanged);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty ShowTrackErrorsProperty = BindableProperty.Create(nameof(ShowTrackErrors), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowTrackErrorsChanged);
    public static readonly BindableProperty EditModeProperty = BindableProperty.Create(nameof(EditMode), typeof(EditModeEnum), typeof(ControlPanelView), EditModeEnum.Move, BindingMode.Default, propertyChanged: OnEditModeChanged);

    public Panel? Panel {
        get => (Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public bool DesignMode {
        get => (bool)GetValue(DesignModeProperty);
        set => SetValue(DesignModeProperty, value);
    }

    public bool ShowGrid {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool Interactive {
        get => (bool)GetValue(InteractiveProperty);
        set => SetValue(InteractiveProperty, value);
    }

    public EditModeEnum EditMode {
        get => (EditModeEnum)GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    public bool ShowTrackErrors {
        get => (bool)GetValue(ShowTrackErrorsProperty);
        set => SetValue(ShowTrackErrorsProperty, value);
    }

    private static async void OnDesignModeChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is ControlPanelView control) {
            control.ShowGrid = control.DesignMode;
            control.DynamicGrid.GestureRecognizers.Clear();
            if (control.DesignMode) {
                // In design mode we need to support drag and drop for the tiles on the screen.
                // ----------------------------------------------------------------------------
                var dropRecogniser = new DropGestureRecognizer();
                dropRecogniser.Drop += control.DropTileOnPanel;
                dropRecogniser.DragOver += control.DragOverTileOnPanel;
                dropRecogniser.DragLeave += control.DragLeaveTileOnPanel;
                control.DynamicGrid.GestureRecognizers.Add(dropRecogniser);

                // In design mode, also support tapping anywhere that is not a tile so we clear selections.
                // ----------------------------------------------------------------------------
                if (control.Interactive || control.DesignMode) {
                    var tapRecogniser = new TapGestureRecognizer();
                    tapRecogniser.Tapped += control.DynamicGridTapped;
                    control.DynamicGrid.GestureRecognizers.Add(tapRecogniser);
                }
            }
            await control.DrawPanel();
        }
    }

    /// <summary>
    ///     If the Panel object is changed, then we need to clear and rebuild the whole Panel
    /// </summary>
    private static async void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is not ControlPanelView control) return;

        // Unsubscribe from the old panel's events to prevent memory leaks and stop listening to it.
        if (oldValue is Panel oldPanel) {
            oldPanel.Entities.CollectionChanged -= control.EntitiesOnCollectionChanged;
            oldPanel.PropertyChanged -= control.OnPanelPropertyChanged;
        }

        // Subscribe to the new panel's events to react to its changes.
        if (newValue is Panel newPanel) {
            newPanel.Entities.CollectionChanged += control.EntitiesOnCollectionChanged;
            newPanel.PropertyChanged += control.OnPanelPropertyChanged;

            control.ClearAllSelectedTiles();
            await control.ForceRefresh();
        }
    }

    private void EntitiesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        ClearAllSelectedTiles();

        // Remove any entities from the grid that were removed from the collection.
        if (e.OldItems is not null) {
            foreach (var oldEntity in e.OldItems.Cast<Entity>()) {
                _logger.LogDebug("Removing Item from Grid: {item}@{col},{row}", oldEntity.EntityName, oldEntity.Col, oldEntity.Row);
                RemoveEntityFromGrid(oldEntity);
            }
        }

        // Add any new entities to the grid that were added to the collection.
        if (e.NewItems is not null) {
            ITile? lastTile = null;
            foreach (var newEntity in e.NewItems.Cast<Entity>()) {
                _logger.LogDebug("Adding Item to Grid: {item}@{col},{row}", newEntity.EntityName, newEntity.Col, newEntity.Row);
                lastTile = AddEntityToGrid(newEntity);
            }

            // Highlight the last item that was added.
            if (lastTile is not null) {
                MarkTileSelected(lastTile);
            }
        }
    }

    /// <summary>
    /// Responds to property changes on the currently assigned Panel object.
    /// </summary>
    private async void OnPanelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (e.PropertyName is nameof(Panel.Cols) or nameof(Panel.Rows)) {
            Dispatcher.Dispatch(async void () => {
                try {
                    await ForceRefresh();
                } catch (Exception ex) {
                    _logger.LogCritical("Error Forcing a Refresh on Col/Row Change: {Message}",ex.Message);
                }
            });
        }
    }

    private static void OnClientChanged(BindableObject bindable, object oldvalue, object newvalue) {
        if (bindable is ControlPanelView control) { }
    }

    private static void OnShowTrackErrorsChanged(BindableObject bindable, object oldvalue, object newvalue) {
        if (bindable is ControlPanelView control) { }
    }

    private static void OnShowGridChanged(BindableObject bindable, object oldvalue, object newvalue) {
        if (bindable is ControlPanelView control) {
            control.DrawGrid();
        }
    }

    private static void OnInteractiveChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is ControlPanelView control) { }
    }

    private static void OnEditModeChanged(BindableObject bindable, object oldvalue, object newvalue) { }
    #endregion
}