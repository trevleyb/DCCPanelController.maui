using System.Collections.Specialized;
using System.ComponentModel;
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
    private const int DoubleTapThreshold = 200;

    private readonly ILogger<ControlPanelView> _logger;
    private readonly PathTracingService _pathTracer = new();
    private readonly HashSet<ITile> _selectedTiles = [];
    private readonly object _tapLock = new();
    private int _currentSelectionIndex;

    [ObservableProperty] private bool _isPanelDrawing = false;

    private enum GestureOwner { None, Tap, LongPress, DragSelect }

    private GestureOwner _gestureOwner = GestureOwner.None;
    private bool _dragSelectionActive; // true once pointer travel passes slop and we’re selecting
    private DateTime _suppressTapsUntilUtc = DateTime.MinValue;
    private const double DragSlopPx = 6; // adjust as needed (logical pixels)
    private Point? _pointerDownPos;
    private TapGestureRecognizer? _gridTap;
    private TouchBehavior? _gridTouch;
    private bool _longPressActive;
    private bool _lpInvokedThisPress; // guard: fire LP once per press

    private int _dragStartCol;
    private int _dragStartRow;
    private int _lastDragCol;
    private int _lastDragRow;

    private double _gridSize;
    private SelectionOutlineDrawable? _selectionOutlineDrawable;
    private GraphicsView? _selectionOutlinegraphicsView;
    private int _tapCount;
    private Timer? _tapTimer;
    private double _viewHeight;
    private double _viewWidth;

    public ControlPanelView() {
        _logger = MauiProgram.ServiceHelper.GetService<ILogger<ControlPanelView>>();
        InitializeComponent();
        SizeChanged += OnGridSizeChanged;
        MainGrid.SizeChanged += OnGridSizeChanged;
    }

    public int Rows => Panel?.Rows ?? 27;
    public int Cols => Panel?.Cols ?? 18;

    public bool IsSelecting { get; set; }
    public bool HasDrawnSelector { get; set; }
    public int StartCol { get; private set; }
    public int StartRow { get; private set; }
    public int EndCol { get; private set; }
    public int EndRow { get; private set; }

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
    public Task ForceRefresh() => DrawPanel(true);

    private async void OnGridSizeChanged(object? sender, EventArgs e) {
        try {
            await DrawPanel();
        } catch (Exception ex) {
            _logger.LogError("OnGridSizeChanged Threw Error: {Message}", ex.Message);
        }
    }

    /// <summary>
    ///     Determines whether the grid size has changed based on the provided width and height parameters.
    /// </summary>
    public bool HasGridSizeChanged(double width, double height) {
        const double epsilon = 0.01;
        if (width < 1.0 || height < 1.0) return false;
        var newGridSize = CalculateGridSize(width, height);
        var difference = Math.Abs(newGridSize - _gridSize);
        return difference > epsilon;
    }

    /// <summary>
    ///     Calculates the optimal grid size based on the specified width and height dimensions.
    /// </summary>
    public double CalculateGridSize(double width, double height) {
        if (width <= 0 || height <= 0) return 1;
        var gridSize = Math.Min(width / Cols, height / Rows);
        gridSize = Math.Floor(gridSize * 100) / 100.0;
        return gridSize;
    }

    /// <summary>
    ///     Renders or refreshes the panel's grid based on the current state, dimensions, and the specified parameters.
    /// </summary>
    private async Task DrawPanel(bool forceRefresh = false,
                                 [CallerMemberName] string memberName = "",
                                 [CallerLineNumber] int sourceLineNumber = 0) {
        if (Panel is null || IsPanelDrawing) return;

        // Console.WriteLine($"DrawPanel: {Panel?.Id}/{Panel?.Guid} | {MainGrid.Width}w x {MainGrid.Height}h | Pending={_pendingFirstDraw} | {memberName}@{sourceLineNumber} ");

        // Only redraw the grid if we absolutely need to. Events may mean that this 
        // is called multiple times, but if we really have not changed, then do not 
        // waste time redrawing and rebuilding the grid. 
        // -------------------------------------------------------------------------
        if (MainGrid.Width < 1.0 || MainGrid.Height < 1.0) return;
        if (!forceRefresh && !HasGridSizeChanged(MainGrid.Width, MainGrid.Height)) return;
        (StartCol, StartRow, EndCol, EndRow) = (-1, -1, 0, 0);

        // Draw the Grid. Make sure we clean up if it has already been drawn first
        // -------------------------------------------------------------------------
        try {
            IsPanelDrawing = true;
            await Task.Delay(10); // Yield to allow UI to update

            ClearAllSelectedTiles();
            RemoveAllTilesFromGrid();

            using (new CodeTimer($"Draw Panel: {Panel?.Id} called from {memberName}@{sourceLineNumber}", DebugMode.IsDebug)) {
                _gridSize = CalculateGridSize(MainGrid.Width, MainGrid.Height);
                _viewWidth = _gridSize * Cols;
                _viewHeight = _gridSize * Rows;

                DynamicGrid.ZIndex = 0;
                DynamicGrid.WidthRequest = _viewWidth;
                DynamicGrid.HeightRequest = _viewHeight;
                DynamicGrid.BackgroundColor = Panel?.PanelBackgroundColor ?? Colors.Transparent;

                // If the grid rows and or columns have changed, then recreate the Dynamic Grid
                // ------------------------------------------------------------------------------
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

                // Add all Entities to the Grid and Draw the Grid Lines
                // ------------------------------------------------------------------------------
                await AddEntitiesToGrid(Panel);
                DrawGrid();
            }
        } finally {
            IsPanelDrawing = false;
        }
    }

    /// <summary>
    ///     Given the Panel list of Entities, add each one as a tile to the panel.
    /// </summary>
    private async Task AddEntitiesToGrid(Panel? panel) {
        if (panel is null) return;
        _pathTracer.ClearTileRegistry();

        DynamicGrid.BatchBegin();
        try {
            foreach (var entity in panel.Entities.OrderBy(x => x.Layer)) {
                AddEntityToGrid(entity);
            }
        } finally {
            DynamicGrid.BatchCommit();
        }
    }

    /// <summary>
    ///     Given an Entity, create a tile and add it to the panel grid.
    /// </summary>
    /// <returns>Returns an instance of the created tile or null if it could not create one. </returns>
    private ITile? AddEntityToGrid(Entity entity) {
        using (new CodeTimer($"Add Entity to Grid: {entity.EntityName}:{entity.Guid} @ {entity.Col},{entity.Row}", false)) {
            var tile = TileFactory.CreateTile(entity, _gridSize, DesignMode ? TileDisplayMode.Design : TileDisplayMode.Normal);
            if (tile is not null) {
                tile.TileChanged += TileOnPropertiesChanged;
                if (tile is ContentView view) {
                    view.ClassId = entity.Guid.ToString();
                    SetTileGestures(tile);
                    SetTileGridPosition(tile);
                    DynamicGrid.Children.Add(view);
                    if (tile is TrackTile trackTile) _pathTracer.RegisterTile(trackTile);
                }
                return tile;
            }
            _logger.LogError("Unable to create the tile for '{Entity}'", entity.EntityName);
            return null;
        }
    }

    private void TileOnPropertiesChanged(object? sender, TileChangedEventArgs e) {
        if (sender is Tile tile) {
            if (e.ChangeType == TileChangeType.Dimensions) {
                SetTileGridPosition(tile);
                tile.ForceRedraw();
            }
        }
    }

    private void SetTileGestures(ITile tile) {
        // If this tile is an interactive tile, then add a gesture recogniser
        // so that when tapped or double-tapped, we can interact with it.
        // --------------------------------------------------------------------
        if (tile is ContentView view) {
            view.Behaviors.Clear();
            view.GestureRecognizers.Clear();
            view.InputTransparent = true;

            // If design mode, we need to be able to drag the tiles around on the design surface
            // but not if we are in operating mode. 
            // ---------------------------------------------------------------------------------
            if (DesignMode) {
                var dragGesture = new DragGestureRecognizer();
                dragGesture.DragStarting += (_, args) => DragTileStarting(args, tile);
                dragGesture.DropCompleted += DragCompleted;
                view.GestureRecognizers.Add(dragGesture);
            }
        }
    }

    private async void DynamicGridLongPress(object? sender, LongPressCompletedEventArgs e) {
        try {
            if (_dragSelectionActive) return; // drag-select wins
            if (_lpInvokedThisPress) return;  // already handled for this press

            _lpInvokedThisPress = true; // mark handled
            _longPressActive = true;
            _gestureOwner = GestureOwner.LongPress;

            // Hard-disable Tap & LongPress to prevent repeat callbacks on hold
            // _gridTap?.SetValue(VisualElement.IsEnabledProperty, false);
            // _gridTouch?.SetValue(VisualElement.IsEnabledProperty, false);

            CancelTapTimer();
            SuppressTapsFor(500); // eat any stray 'Tapped' on finger up

            var tapTimerState = new TapTimerState(sender, (StartCol, StartRow));
            if (DesignMode) OnDesignModeLongPress(tapTimerState);
            else OnOperateModeLongPress(tapTimerState);
        } catch (Exception ex) {
            Console.WriteLine("Error in long press: " + ex.Message);
        }

        // Re-enable in PointerReleased/Exited
    }

    private async void DynamicGridTapped(object? sender, TappedEventArgs e) {
        if (_longPressActive) return;
        if (TapsSuppressed()) return;                        // long-press just finished, ignore stray tap
        if (_gestureOwner == GestureOwner.LongPress) return; // long-press owns this sequence
        if (_dragSelectionActive) return;                    // drag-select in progress takes precedence

        lock (_tapLock) {
            var pos = GetGridPosition(e.GetPosition(DynamicGrid)) ?? (-1, -1);
            _tapCount++;
            _gestureOwner = GestureOwner.Tap; // claim ownership as tap (tentative)
            _tapTimer?.Dispose();
            _tapTimer = new Timer(DynamicGridTapTimerElapsed,
                                  new TapTimerState(sender, pos),
                                  DoubleTapThreshold,
                                  Timeout.Infinite);
        }
    }

    private void DynamicGridTapTimerElapsed(object? state) {
        if (state is not TapTimerState tapTimerState) return;

        int count;
        lock (_tapLock) {
            count = _tapCount;
            _tapCount = 0;
            _tapTimer?.Dispose();
            _tapTimer = null;
        }

        // If long-press took over during the wait, ignore the pending tap(s)
        if (_gestureOwner != GestureOwner.Tap) return;

        // Dispatch back to UI thread
        MainThread.BeginInvokeOnMainThread(() => {
            switch (count) {
            case 1:
                if (DesignMode) OnDesignModeSingleTap(tapTimerState);
                else OnOperateModeSingleTap(tapTimerState);
                break;

            case 2:
                if (DesignMode) OnDesignModeDoubleTap(tapTimerState);
                else OnOperateModeDoubleTap(tapTimerState);
                break;

            case 3:
                Console.WriteLine("Tapped 3 times");
                break;
            }

            _gestureOwner = GestureOwner.None; // release ownership
        });
    }

    private async void OnDesignModeSingleTap(TapTimerState tapTimerState) {
        Console.WriteLine($"SINGLE TAP: DESIGN MODE => @{tapTimerState.Col},{tapTimerState.Row}");

        // look up the tile if it is in this grid
        // -------------------------------------------------------------------
        var tilesAtPosition = TilesInGrid(tapTimerState.Position);
        if (tilesAtPosition.Count == 0) {
            ClearAllSelectedTiles();
            OnTileSelected(_tapCount);
            return;
        }

        // Determine if any tile at this position is currently selected
        // -------------------------------------------------------------------
        var selectedIndexAtPos = tilesAtPosition.FindIndex(t => t.IsSelected);

        // Case 1: None selected at this position -> unselect all tiles globally,
        // then select the first at this position.
        // -----------------------------------------------------------------------
        if (selectedIndexAtPos == -1) {
            MarkTileSelected(tilesAtPosition[0]);
            _currentSelectionIndex = 0;
        }

        // Case 2: A tile is selected at this position and there is only one tile -> unselect it.
        // -------------------------------------------------------------------------------------
        else if (tilesAtPosition.Count == 1) {
            MarkTileUnSelected(tilesAtPosition[0]);
            _currentSelectionIndex = -1;
        }

        // Case 3: A tile is selected at this position and there are multiple tiles -> cycle selection.
        // -------------------------------------------------------------------------------------------
        else {
            var nextIndex = (selectedIndexAtPos + 1) % tilesAtPosition.Count;
            foreach (var t in tilesAtPosition) {
                if (t.IsSelected) MarkTileUnSelected(t);
            }

            // Select the next tile in the stack
            MarkTileSelected(tilesAtPosition[nextIndex]);
            _currentSelectionIndex = nextIndex;
        }

        OnTileSelected(_tapCount);
    }

    private async void OnDesignModeDoubleTap(TapTimerState tapTimerState) {
        Console.WriteLine($"DOUBLE TAP: DESIGN MODE => @{tapTimerState.Col},{tapTimerState.Row}");
        var tile = TilesInGrid(tapTimerState.Position).FirstOrDefault();

        // If we double-tapped on an actual tile, then unmark all tiles and mark this one
        // ------------------------------------------------------------------------------
        if (tile != null) {
            ClearAllSelectedTiles();
            MarkTileSelected(tile);
            OnTileSelected(_tapCount);
        }

        // If there was no tile in this grid position, then use the Edit Mode to determine 
        // what we should do. Either move the tile to this position, or clone the selected tile
        else {
            Console.WriteLine("MOVE OR COPY TILES");
            HandleDoubleTapEmptyCellMoveOrCopy(tapTimerState);
        }
    }

    private async void OnDesignModeLongPress(TapTimerState tapTimerState) {
        Console.WriteLine($"LONG PRESS: DESIGN MODE => @{tapTimerState.Col},{tapTimerState.Row}");
    }

    private async void OnOperateModeSingleTap(TapTimerState tapTimerState) {
        Console.WriteLine($"SINGLE TAP: OPERATE MODE => @{tapTimerState.Col},{tapTimerState.Row}");

        // Find the tile that was tapped and raise an event for that tile. 
        // In operating mode, it can only be an interactive tile 
        // ---------------------------------------------------------------------------
        var interactiveTile = InteractiveTilesInGrid(tapTimerState.Position).FirstOrDefault();
        if (interactiveTile != null) OnTileTapped(interactiveTile, 1);
    }

    private async void OnOperateModeDoubleTap(TapTimerState tapTimerState) {
        Console.WriteLine($"DOUBLE TAP: OPERATE MODE => @{tapTimerState.Col},{tapTimerState.Row}");

        // Find the tile that was tapped and raise an event for that tile. 
        // In operating mode, it can only be an interactive tile 
        // ---------------------------------------------------------------------------
        var interactiveTile = InteractiveTilesInGrid(tapTimerState.Position).FirstOrDefault();
        if (interactiveTile != null) OnTileTapped(interactiveTile, 2);
    }

    private async void OnOperateModeLongPress(TapTimerState tapTimerState) {
        Console.WriteLine($"LONG PRESS: OPERATE MODE => @{tapTimerState.Col},{tapTimerState.Row}");
        try {
            var tile = TrackTilesInGrid(tapTimerState.Position).FirstOrDefault();
            if (tile is TrackTile trackTile) await _pathTracer.StartPathTracing(trackTile);
        } catch (Exception ex) {
            Console.WriteLine($"Error in OnOperateModeLongPress: {ex.Message}");
        }
    }

    /// <summary>
    ///     This is a clean up route. If we redraw the grid, remove each tile from the grid first and ensure
    ///     we have removed events and gestures.
    /// </summary>
    private void RemoveAllTilesFromGrid() {
        var children = DynamicGrid.Children.OfType<ITile>().ToList();
        DynamicGrid.BatchBegin();
        foreach (var tile in children) RemoveTileFromGrid(tile);
        DynamicGrid.BatchCommit();
    }

    /// <summary>
    ///     Find all tiles in the grid that match the offset of the provided tile
    /// </summary>
    private List<ITile> TilesInGrid(ITile tile) {
        return TilesInGrid(tile.Entity.Col, tile.Entity.Row);
    }

    /// <summary>
    ///     Find all tiles in the grid that match the offset of col, row
    /// </summary>
    private List<ITile> TilesInGrid((int col, int row) grid) => TilesInGrid(grid.col, grid.row);

    private List<ITile> TilesInGrid(int col, int row) {
        return DynamicGrid.Children.OfType<ITile>()
                          .Where(x => x.Entity.Col == col && x.Entity.Row == row)
                          .OrderByDescending(x => x.Entity.Layer) // Highest layer first for selection
                          .ToList();
    }

    private List<ITile> InteractiveTilesInGrid((int col, int row) grid) => InteractiveTilesInGrid(grid.col, grid.row);

    private List<ITile> InteractiveTilesInGrid(int col, int row) {
        return DynamicGrid.Children.OfType<ITile>()
                          .Where(x => x.Entity is IInteractiveEntity &&
                                      x.Entity.Col == col &&
                                      x.Entity.Row == row)
                          .OrderByDescending(x => x.Entity.Layer) // Highest layer first for selection
                          .ToList();
    }

    private List<ITile> TrackTilesInGrid((int col, int row) grid) => TrackTilesInGrid(grid.col, grid.row);

    private List<ITile> TrackTilesInGrid(int col, int row) {
        return DynamicGrid.Children.OfType<ITile>()
                          .Where(x => x.Entity is ITrackEntity &&
                                      x.Entity is not IInteractiveEntity &&
                                      x.Entity.Col == col &&
                                      x.Entity.Row == row)
                          .OrderByDescending(x => x.Entity.Layer) // Highest layer first for selection
                          .ToList();
    }

    private bool IsTileInGrid((int col, int row) grid) => TilesInGrid(grid.col, grid.row).Count > 0;
    private bool IsTileInGrid(int col, int row) => TilesInGrid(col, row).Count > 0;
    private bool IsInteractiveTileInGrid((int col, int row) grid) => InteractiveTilesInGrid(grid.col, grid.row).Count > 0;
    private bool IsInteractiveTileInGrid(int col, int row) => InteractiveTilesInGrid(col, row).Count > 0;
    private bool IsTrackTileInGrid((int col, int row) grid) => TrackTilesInGrid(grid.col, grid.row).Count > 0;
    private bool IsTrackTileInGrid(int col, int row) => TrackTilesInGrid(col, row).Count > 0;

    /// <summary>
    ///     Remove an individual tile from the grid.
    /// </summary>
    private void RemoveTileFromGrid(ITile tile) {
        tile.TileChanged -= TileOnPropertiesChanged;
        RemoveEntityFromGrid(tile.Entity);
        (tile as IDisposable)?.Dispose();
        OnTileChanged(tile);
    }

    /// <summary>
    ///     Remove an Entity from the Grid
    /// </summary>
    private void RemoveEntityFromGrid(Entity entity) {
        var views = DynamicGrid.Children
                               .OfType<Microsoft.Maui.Controls.View>()
                               .Where(x => x.ClassId == entity.Guid.ToString())
                               .ToList();

        foreach (var view in views) {
            if (view is ITile tile) {
                tile.TileChanged -= TileOnPropertiesChanged;
                (tile as IDisposable)?.Dispose();
                MarkTileUnSelected(tile);
            }
            view.GestureRecognizers.Clear();
            DynamicGrid.Remove(view);
            if (view is TrackTile trackTile) _pathTracer.UnregisterTile(trackTile);
        }
    }

    private void ResizeTrack(ITile? tile, int newCol, int newRow) {
        if (tile is null) return; // Only resize tracks

        // Work relative to the drag start position
        if (newCol >= _dragStartCol) { // Dragging right - increase width and keep column the same
            tile.Entity.Col = _dragStartCol;
            tile.Entity.Width = newCol - _dragStartCol + 1;
        } else { // Dragging left - move column left and increase width
            tile.Entity.Col = newCol;
            tile.Entity.Width = _dragStartCol - newCol + 1;
        }

        if (newRow >= _dragStartRow) { // Dragging down - increase height and keep row the same
            tile.Entity.Row = _dragStartRow;
            tile.Entity.Height = newRow - _dragStartRow + 1;
        } else { // Dragging up - move row up and increase height
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
    protected (int Col, int Row)? GetGridPosition(Point? point) {
        if (point is { } tapPosition) return GetGridPosition(tapPosition.X, tapPosition.Y);
        return (0, 0);
    }

    protected (int Col, int Row)? GetGridPosition(double posX, double posY) {
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
        var row = (int)(posY / cellHeight);
        var col = (int)(posX / cellWidth);

        // Ensure indices are within bounds
        row = Math.Min(row, rowCount - 1);
        col = Math.Min(col, colCount - 1);

        return (col, row);
    }
    #endregion

    #region Draw Grid when in Design Mode
    private void DrawGrid() {
        RemoveGrid();
        if (ShowGrid) {
            var gridLines = new GridLinesDrawable(Rows, Cols, GridColor);
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
        RemoveChildView("GridLines");
    }

    private void RemoveChildView(string classID) {
        if (ControlPanelLayout.Children.Count >= 1) {
            var graphicsViewToRemove = ControlPanelLayout.Children.OfType<GraphicsView>().Where(x => x.ClassId == classID).ToList();
            foreach (var view in graphicsViewToRemove) {
                ControlPanelLayout.Children.Remove(view);
            }
        }
    }
    #endregion

    #region Support Marking and UnMarking Tiles on the Panel
    /// <summary>
    ///     Mark a tile as selected, and put a border around it
    /// </summary>
    public void MarkTileSelected(ITile tile) {
        _selectedTiles.Add(tile);
        HighlightCell(tile.Entity.Col, tile.Entity.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.Selected);
        tile.IsSelected = true;
        OnTileSelected(0);
    }

    /// <summary>
    ///     Unmark a tile, remove the border
    /// </summary>
    public void MarkTileUnSelected(ITile tile) {
        _selectedTiles.Remove(tile);
        UnHighlightCell(tile.Entity.Col, tile.Entity.Row);
        tile.IsSelected = false;
        OnTileSelected(0);
    }

    /// <summary>
    ///     There are times, such as when we rotate a tile, that the bounds may have
    ///     changed, and we need to re-mark the tile. This code will unmark and
    ///     remark the tiles where the width or height > 1. This is done
    ///     without calling Mark/UnMark as we do not want to event that the
    ///     tile was marked or unmarked.
    /// </summary>
    public void ReMarkRotatedSelectedTiles() {
        foreach (var tile in _selectedTiles.Where(x => x.Entity.Width > 1 || x.Entity.Height > 1)) {
            HighlightCell(tile.Entity.Col, tile.Entity.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.Selected);
        }
    }

    public void UnMarkRotatedSelectedTiles() {
        foreach (var tile in _selectedTiles.Where(x => x.Entity.Width > 1 || x.Entity.Height > 1)) {
            UnHighlightCell(tile.Entity.Col, tile.Entity.Row);
        }
    }

    /// <summary>
    ///     Clear all tiles that are marked as selected
    /// </summary>
    public void ClearAllSelectedTiles() {
        foreach (var tile in _selectedTiles) MarkTileUnSelected(tile);
        var children = DynamicGrid.Children.Where(x => x is Border border && x.Parent is Grid && border.ClassId == "CellHighlight").ToList();
        foreach (var child in children) DynamicGrid.Remove(child);
        _selectedTiles.Clear();
        OnTileSelected(0);
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
            CellHighlightAction.Selecting   => Colors.LightSkyBlue,
            _                               => Colors.Red
        };

        var border = new Border {
            ClassId = "CellHighlight",
            Stroke = color,
            StrokeThickness = 2,
            BackgroundColor = color.WithAlpha(0.25f),
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

    #region Support for the Grid Selection
    private void UpdateSelectorView(int startCol, int startRow, int endCol, int endRow) {
        if (_selectionOutlineDrawable is null || _selectionOutlinegraphicsView is null) {
            _selectionOutlineDrawable = new SelectionOutlineDrawable();
            _selectionOutlineDrawable.SetBounds(startCol, startRow, endCol, endRow, _gridSize);
            _selectionOutlinegraphicsView = new GraphicsView {
                InputTransparent = true,
                Drawable = _selectionOutlineDrawable,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                ClassId = "SelectorView"
            };

            // Add the GraphicsView directly to the AbsoluteLayout
            AbsoluteLayout.SetLayoutBounds(_selectionOutlinegraphicsView, new Rect(0.5, 0.5, _viewWidth, _viewHeight));
            AbsoluteLayout.SetLayoutFlags(_selectionOutlinegraphicsView, AbsoluteLayoutFlags.PositionProportional);
            ControlPanelLayout.Children.Add(_selectionOutlinegraphicsView);
            _selectionOutlinegraphicsView.Invalidate();
        }

        if (_selectionOutlineDrawable is null || _selectionOutlinegraphicsView is null) return;
        _selectionOutlineDrawable.SetBounds(startCol, startRow, endCol, endRow, _gridSize);
        _selectionOutlinegraphicsView.Invalidate();
    }

    private void RemoveSelectorView() {
        _selectionOutlineDrawable = null;
        RemoveChildView("SelectorView");
    }

    private void DynamicGridPointerPressed(object? sender, PointerEventArgs e) {
        _lpInvokedThisPress = false;
        _longPressActive = false;

        var cell = GetGridPosition(e.GetPosition(DynamicGrid));
        if (cell is { } gridCell) {
            if (IsTileInGrid(gridCell.Col, gridCell.Row)) return;

            StartCol = EndCol = gridCell.Col;
            StartRow = EndRow = gridCell.Row;
            _pointerDownPos = e.GetPosition(DynamicGrid);
            _dragSelectionActive = false; // not active yet—wait for slop
            IsSelecting = true;
        }
    }

    private void DynamicGridPointerMoved(object? sender, PointerEventArgs e) {
        if (!IsSelecting) return;

        var pos = e.GetPosition(DynamicGrid);
        if (!_dragSelectionActive && _pointerDownPos is { } p0 && pos is { } p1) {
            var dx = Math.Abs(p1.X - p0.X);
            var dy = Math.Abs(p1.Y - p0.Y);
            if (dx >= DragSlopPx || dy >= DragSlopPx) {
                _dragSelectionActive = true;
                _gestureOwner = GestureOwner.DragSelect; // drag-select owns the sequence now
                CancelTapTimer();                        // cancel any pending taps
            }
        }

        var cell = GetGridPosition(pos);
        if (cell is { } gridCell) {
            EndCol = gridCell.Col;
            EndRow = gridCell.Row;
        }

        var minCol = Math.Min(StartCol, EndCol);
        var maxCol = Math.Max(StartCol, EndCol);
        var minRow = Math.Min(StartRow, EndRow);
        var maxRow = Math.Max(StartRow, EndRow);

        if (minCol != maxCol || minRow != maxRow) {
            UpdateSelectorView(minCol, minRow, maxCol, maxRow);
            MarkTilesSelectedInGrid(minCol, minRow, maxCol, maxRow);
        }
    }

    private void DynamicGridPointerReleased(object? sender, PointerEventArgs e) {
        if (IsSelecting) RemoveSelectorView();

        _gestureOwner = GestureOwner.None;
        _dragSelectionActive = false;
        _pointerDownPos = null;
        IsSelecting = false;

        if (_longPressActive) {
            SuppressTapsFor(500);
            _longPressActive = false;
        }

        // Re-enable recognizers and clear LP guard regardless
        // _gridTap?.SetValue(VisualElement.IsEnabledProperty, true);
        // _gridTouch?.SetValue(VisualElement.IsEnabledProperty, true);
        _lpInvokedThisPress = false;
    }

    private void DynamicGridPointerExited(object? sender, PointerEventArgs e) {
        if (IsSelecting) {
            RemoveSelectorView();
            ClearAllSelectedTiles();
        }

        _gestureOwner = GestureOwner.None;
        _dragSelectionActive = false;
        _pointerDownPos = null;
        IsSelecting = false;

        _longPressActive = false;

        //_gridTap?.SetValue(VisualElement.IsEnabledProperty, true);
        //_gridTouch?.SetValue(VisualElement.IsEnabledProperty, true);
        _lpInvokedThisPress = false;
    }

    private void MarkTilesSelectedInGrid(int startCol, int startRow, int endCol, int endRow) {
        // @formatter:off
        var unselectedTilesInRange = DynamicGrid.Children
            .OfType<ITile>().Where(tile =>
                tile.Entity.Col >= startCol && tile.Entity.Col <= endCol &&
                tile.Entity.Row >= startRow && tile.Entity.Row <= endRow &&
                !tile.IsSelected)
                .ToList();

        var selectedTilesOutsideRange = DynamicGrid.Children
            .OfType<ITile>().Where(tile =>
                tile.IsSelected &&
                (tile.Entity.Col < startCol || tile.Entity.Col > endCol ||
                tile.Entity.Row < startRow || tile.Entity.Row > endRow))
                .ToList();
        // @formatter:on

        foreach (var tile in unselectedTilesInRange) MarkTileSelected(tile);
        foreach (var tile in selectedTilesOutsideRange) MarkTileUnSelected(tile);
    }
    #endregion

    #region Move or Copy Selected
    private void HandleDoubleTapEmptyCellMoveOrCopy(TapTimerState tap) {
        var anchorCol = tap.Col;
        var anchorRow = tap.Row;

        // Must tap an empty cell to trigger move/copy by requirement
        if (IsTileInGrid(anchorCol, anchorRow)) return;
        if (_selectedTiles.Count == 0) return; // nothing to move/copy

        // Only allow recognized modes
        if (EditMode is not (EditModeEnum.Move or EditModeEnum.Copy)) return;

        if (!CanPlaceSelectionAt(anchorCol, anchorRow, EditMode)) {
            // Optional: briefly flash invalid destination rectangles if you want UX feedback
            Console.WriteLine("Cannot place selection: destination blocked or out of bounds");
            return;
        }

        if (EditMode == EditModeEnum.Move) {
            PerformMoveSelection(anchorCol, anchorRow);
        } else {
            PerformCopySelection(anchorCol, anchorRow);
        }
    }

    // Move the whole selection so top-left lands at anchorCol/Row
    private void PerformMoveSelection(int anchorCol, int anchorRow) {
        if (_selectedTiles.Count == 0) return;
        var (minCol, minRow) = GetSelectionTopLeft()!.Value;

        // Move: update entity rows/cols in-place, then refresh grid positions
        UnMarkRotatedSelectedTiles();
        foreach (var tile in _selectedTiles) {
            //UnHighlightCell(tile);

            var e = tile.Entity;
            e.Col = anchorCol + (e.Col - minCol);
            e.Row = anchorRow + (e.Row - minRow);
            SetTileGridPosition(tile);
            tile.ForceRedraw();
            OnTileChanged(tile);
            
            //HighlightCell(tile, CellHighlightAction.Selected);
        }
        ReMarkRotatedSelectedTiles();
    }

    // Copy the whole selection to anchorCol/Row (select the copies)
    private void PerformCopySelection(int anchorCol, int anchorRow) {
        
        if (_selectedTiles.Count == 0 || Panel is null) return;
        var (minCol, minRow) = GetSelectionTopLeft()!.Value;

        UnMarkRotatedSelectedTiles();
        var tilesToCopy = _selectedTiles.ToList();
        foreach (var tile in tilesToCopy) {
            var e = tile.Entity;
            var newEntity = Panel.CreateEntityFrom(e);
            newEntity.Col = anchorCol + (e.Col - minCol);
            newEntity.Row = anchorRow + (e.Row - minRow);
            Panel.AddEntity(newEntity);
            OnTileChanged(tile);
        }
        ReMarkRotatedSelectedTiles();
    }
    
    // Returns the top-left (min col,row) of the current selection.
    // Returns null if no tiles are selected.
    private (int minCol, int minRow)? GetSelectionTopLeft() {
        if (_selectedTiles.Count == 0) return null;
        var minCol = _selectedTiles.Min(t => t.Entity.Col);
        var minRow = _selectedTiles.Min(t => t.Entity.Row);
        return (minCol, minRow);
    }

    // Axis-aligned rectangle overlap test
    private static bool RectsOverlap(int aCol, int aRow, int aW, int aH,
                                     int bCol, int bRow, int bW, int bH) {
        return aCol < bCol + bW && aCol + aW > bCol &&
               aRow < bRow + bH && aRow + aH > bRow;
    }

    // Is the destination area (col,row,width,height) fully inside the panel?
    private bool InBounds(int col, int row, int w, int h) {
        return col >= 0 && row >= 0 &&
               w >= 1 && h >= 1 &&
               col + w <= Cols &&
               row + h <= Rows;
    }

    // Are ALL destination rectangles for the (copy/move) valid and free?
    // - anchorCol/Row = where the "top-left" of the selection will land
    // - If mode == Move we ignore collisions with the currently selected tiles (because they vacate)
    private bool CanPlaceSelectionAt(int anchorCol, int anchorRow, EditModeEnum mode) {
        if (_selectedTiles.Count == 0) return false;

        var selTopLeft = GetSelectionTopLeft();
        if (selTopLeft is null) return false;
        var (minCol, minRow) = selTopLeft.Value;

        // Snapshot of existing tiles to test collisions against
        var existing = DynamicGrid.Children.OfType<ITile>().ToList();

        foreach (var tile in _selectedTiles) {
            var e = tile.Entity;

            // Compute destination top-left for this tile (preserve relative offset)
            var destCol = anchorCol + (e.Col - minCol);
            var destRow = anchorRow + (e.Row - minRow);

            // Bounds check
            if (!InBounds(destCol, destRow, e.Width, e.Height)) return false;

            // Collision check against *other* tiles
            foreach (var other in existing) {
                if (mode == EditModeEnum.Move && _selectedTiles.Contains(other)) continue; // selected tiles are moving away; ignore their current rects
                var oe = other.Entity;
                if (RectsOverlap(destCol, destRow, e.Width, e.Height, oe.Col, oe.Row, oe.Width, oe.Height)) return false;
            }
        }
        return true;
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
        if (EditMode == EditModeEnum.Size && tile.Entity is not IDrawingEntity) {
            args.Cancel = true;
            return;
        }

        _gestureOwner = GestureOwner.DragSelect; // treat tile drag similar to owning the sequence
        CancelTapTimer();
        SuppressTapsFor(200);

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
        if (sender is DragGestureRecognizer { Parent : ITile tile }) {
            tile.ForceRedraw();
            ClearAllSelectedTiles();
        }
    }

    /// <summary>
    ///     Support dropping the dragged tile onto the panel in a new position (or the same position)
    /// </summary>
    private void DropTileOnPanel(object? sender, DropEventArgs e) {
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
                //_logger.LogInformation("DropTileOnPanel Source='{Source}' Tile='{Tile}' Position='{Col},{Row}'", source, tile.Entity.EntityName ?? "Undefined", position.Col, position.Row);

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
                                      )
                                     .ToList();

        // If there are noe tiles in the grid at the moment, then go-for-it
        // ----------------------------------------------------------------
        if (tilesInGrid.Count == 0) return false;

        // If there is already an Interactive tile in the grid and this tile is an interactive tile
        // then you cannot add it as we cannot have 2 interactive tiles in the same location
        // ----------------------------------------------------------------
        return tilesInGrid.Any(x => x.Entity is IInteractiveEntity) && tile.Entity is IInteractiveEntity;
    }
    #endregion

    #region Bindable Properties
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty InteractiveProperty = BindableProperty.Create(nameof(Interactive), typeof(bool), typeof(ControlPanelView), true, BindingMode.Default, propertyChanged: OnInteractiveChanged);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty GridColorProperty = BindableProperty.Create(nameof(GridColor), typeof(Color), typeof(ControlPanelView), Colors.DarkGray, BindingMode.Default, propertyChanged: OnShowGridChanged);
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

    public Color GridColor {
        get => (Color)GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
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
            control.Behaviors.Clear();

            if (control.DesignMode) {
                // In design mode we need to support drag and drop for the tiles on the screen.
                // ----------------------------------------------------------------------------
                var dropRecogniser = new DropGestureRecognizer();
                dropRecogniser.Drop += control.DropTileOnPanel;
                dropRecogniser.DragOver += control.DragOverTileOnPanel;
                dropRecogniser.DragLeave += control.DragLeaveTileOnPanel;
                control.DynamicGrid.GestureRecognizers.Add(dropRecogniser);

                var pointerRecognizer = new PointerGestureRecognizer();
                pointerRecognizer.PointerPressed += control.DynamicGridPointerPressed;
                pointerRecognizer.PointerMoved += control.DynamicGridPointerMoved;
                pointerRecognizer.PointerReleased += control.DynamicGridPointerReleased;
                pointerRecognizer.PointerExited += control.DynamicGridPointerExited;
                control.DynamicGrid.GestureRecognizers.Add(pointerRecognizer);
            }

            // In design mode, also support tapping anywhere that is not a tile so we clear selections.
            // ----------------------------------------------------------------------------
            control._gridTap = new TapGestureRecognizer();
            control._gridTap.Tapped += control.DynamicGridTapped;
            control.DynamicGrid.GestureRecognizers.Add(control._gridTap);

            // Long-press via TouchBehavior (keep a reference)
            control._gridTouch = new TouchBehavior();
            control._gridTouch.LongPressCompleted += control.DynamicGridLongPress;
            control.DynamicGrid.Behaviors.Add(control._gridTouch);

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
    ///     Responds to property changes on the currently assigned Panel object.
    /// </summary>
    private async void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is nameof(Panel.Cols) or nameof(Panel.Rows)) {
            Dispatcher.Dispatch(async void () => {
                try {
                    await ForceRefresh();
                } catch (Exception ex) {
                    _logger.LogCritical("Error Forcing a Refresh on Col/Row Change: {Message}", ex.Message);
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

    #region Helpers
    public enum CellHighlightAction {
        Selected,
        DragInvalid,
        DragValid,
        Resize,
        Selecting
    }

    public string GetEditModeIconFilename =>
        EditMode switch {
            EditModeEnum.Copy => "copy.png",
            EditModeEnum.Move => "move.png",
            EditModeEnum.Size => "crop.png",
            _                 => "move.png"
        };

    // Helper to carry both sender and tap args into the timer callback
    private sealed class TapTimerState {
        public object? Sender { get; }
        public (int Col, int Row) Position { get; }
        public int Col { get; }
        public int Row { get; }

        public TapTimerState(object? sender, (int col, int row) gridPos) : this(sender, gridPos.col, gridPos.row) { }

        public TapTimerState(object? sender, int col, int row) {
            Sender = sender;
            Col = col;
            Row = row;
            Position = (col, row);
        }
    }

    private void CancelTapTimer() {
        lock (_tapLock) {
            _tapCount = 0;
            _tapTimer?.Dispose();
            _tapTimer = null;
        }
    }

    private bool TapsSuppressed() => DateTime.UtcNow < _suppressTapsUntilUtc;

    private void SuppressTapsFor(int ms) {
        _suppressTapsUntilUtc = DateTime.UtcNow.AddMilliseconds(ms);
    }
    #endregion
}