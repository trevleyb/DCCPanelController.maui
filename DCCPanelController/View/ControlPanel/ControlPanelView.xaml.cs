using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.PathFinder;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Layouts;
#if IOS || MACCATALYST
using UIKit;
#endif

namespace DCCPanelController.View.ControlPanel;

[ObservableObject]
public partial class ControlPanelView {
    private const bool ShowCodeTimer = false;
    [ObservableProperty] private bool _isPanelDrawing;

    #region Instance Variables and Properties
    private int _lastDragCol;
    private int _lastDragRow;
    private double _gridSize;
    private double _viewHeight;
    private double _viewWidth;

    private GridGestureHelper? _gridGestures;
    private readonly AbsoluteLayout _panelSurface = new AbsoluteLayout();
    private readonly Grid _dynamicGrid = new Grid();
    private readonly DrawGridLines _overlayGridLines = new DrawGridLines();
    private readonly DrawGridSelection _overlayGridSelection = new DrawGridSelection();
    private readonly DrawGridHighlights _overlayGridHighlights = new DrawGridHighlights();

    private CancellationTokenSource? _sizeChangedDebounceCts;
    private const int SizeChangedDebounceMs = 80; // try 50–100
    private Size _lastCommittedSize;

    private readonly ILogger<ControlPanelView> _logger;
    private readonly PathTracingService _pathTracer = new();
    private readonly HashSet<ITile> _selectedTiles = [];

    public int Rows => Panel?.Rows ?? 27;
    public int Cols => Panel?.Cols ?? 18;
    public bool HasDrawnSelector { get; set; }

    public ControlPanelView() {
        _logger = MauiProgram.ServiceHelper.GetService<ILogger<ControlPanelView>>();
        InitializeComponent();
        SetupDynamicGridGestures(_dynamicGrid);
        SetupGridOverlays();
        SizeChanged += OnSizeChangedDebounced;
    }
    #endregion

    #region Setup the Grid Overlays
    private void SetupGridOverlays() {
        // Create the Surface of the Panel
        // -------------------------------------------------------------
        AbsoluteLayout.SetLayoutFlags(_panelSurface, AbsoluteLayoutFlags.All);
        AbsoluteLayout.SetLayoutBounds(_panelSurface, new Rect(0, 0, 1, 1));

        // Create the Dynamic Grid that our Tiles will render in
        // ---------------------------------------------------------------
        _dynamicGrid.HorizontalOptions = LayoutOptions.Fill;
        _dynamicGrid.VerticalOptions = LayoutOptions.Fill;
        AbsoluteLayout.SetLayoutFlags(_dynamicGrid, AbsoluteLayoutFlags.All);
        AbsoluteLayout.SetLayoutBounds(_dynamicGrid, new Rect(0, 0, 1, 1));
        _panelSurface.Children.Add(_dynamicGrid);

        // Add overlays (also fill the surface)
        // ---------------------------------------------------------------
        foreach (var v in new GraphicsView[] { _overlayGridLines, _overlayGridSelection, _overlayGridHighlights }) {
            v.InputTransparent = true;
            v.HorizontalOptions = LayoutOptions.Fill;
            v.VerticalOptions = LayoutOptions.Fill;
            v.ZIndex = 10;
            AbsoluteLayout.SetLayoutFlags(v, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(v, new Rect(0, 0, 1, 1));
            _panelSurface.Children.Add(v);
        }

        // Finally add the surface where the grid used to be:
        // ---------------------------------------------------------------
        ControlPanelLayout.Children.Add(_panelSurface);

        // 5) Nudge a first repaint after size is known
        _panelSurface.SizeChanged += (_, _) => {
            if (_panelSurface.Width <= 0 || _panelSurface.Height <= 0) return;
            _overlayGridLines.Invalidate();
            _overlayGridSelection.Invalidate();
            _overlayGridHighlights.Invalidate();
        };
    }
    #endregion

    #region Manage the Layout changing in size
    private void OnSizeChangedDebounced(object? sender, EventArgs e) {
        if (Width <= 1 || Height <= 1) return;

        // Cancel any pending commit and schedule a new one
        // ------------------------`--------------------------------------
        Interlocked.Exchange(ref _sizeChangedDebounceCts, null)?.Cancel();
        var cts = new CancellationTokenSource();
        _sizeChangedDebounceCts = cts;
        _ = DebounceSizeCommitAsync(cts.Token);
    }

    private async Task DebounceSizeCommitAsync(CancellationToken token) {
        try {
            await Task.Delay(SizeChangedDebounceMs, token);
            if (token.IsCancellationRequested) return;

            await MainThread.InvokeOnMainThreadAsync(() => {
                var newSize = new Size(Width, Height);
                if (!HasMeaningfulSizeChange(newSize, _lastCommittedSize)) return Task.CompletedTask;
                _lastCommittedSize = newSize;
                return DrawPanel();
            });
        } catch (TaskCanceledException) { /* expected */
        }
    }

    private static bool HasMeaningfulSizeChange(Size a, Size b) {
        const double minPixelDelta = 1.0; // or use 2–3 px if needed
        var result = Math.Abs(a.Width - b.Width) >= minPixelDelta
                  || Math.Abs(a.Height - b.Height) >= minPixelDelta;
        return result;
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args) {
        base.OnHandlerChanging(args);
        if (args.NewHandler is null) {
            SizeChanged -= OnSizeChangedDebounced;
            try {
                Interlocked.Exchange(ref _sizeChangedDebounceCts, null)?.Cancel();
            } catch { /* ignore but should not happen */
            }
        }
    }
    #endregion

    #region Setup the Gestures and Manage them
    // Events that consumers (Panel Viewer or Editor) can subscribe to
    // -------------------------------------------------------------------------
    public event EventHandler<TileSelectedEventArgs>? TileChanged;
    public event EventHandler<TileSelectedEventArgs>? TileSelected;
    public event EventHandler<TileSelectedEventArgs>? TileTapped;

    // Helpers for raising these events
    // -------------------------------------------------------------------------
    private void OnTileChanged(ITile tile) => TileChanged?.Invoke(this, new TileTappedEventArgs(tile, 0));
    private void OnTileTapped(ITile tile, int tapCount) => TileTapped?.Invoke(this, new TileTappedEventArgs(tile, tapCount));
    private void OnTileSelected(int tapCount) => TileSelected?.Invoke(this, new TileSelectedEventArgs(_selectedTiles, tapCount));

    /// <summary>
    /// Set up the gesture recogniser helper which manages events and works out the type
    /// of event (Single, Double, Long Press) or Selection, and raises appropriate events. 
    /// </summary>
    private void SetupDynamicGridGestures(Grid dynamicGrid) {
        _gridGestures = new GridGestureHelper(dynamicGrid);
        _gridGestures.SingleTap += GridGesturesOnSingleTap;
        _gridGestures.DoubleTap += GridGesturesOnDoubleTap;
        _gridGestures.LongPress += GridGesturesOnLongPress;

        _gridGestures.GridSelectionStarted += GridGesturesOnGridSelectionStarted;
        _gridGestures.GridSelectionChanged += GridGesturesOnGridSelectionChanged;
        _gridGestures.GridSelectionCompleted += GridGesturesOnGridSelectionCompleted;
        _gridGestures.GridSelectionCancelled += GridGesturesOnGridSelectionCancelled;

        _gridGestures.TileDragStarted += GridGesturesOnTileDragStarted;
        _gridGestures.TileDragMoved += GridGesturesOnTileDragMoved;
        _gridGestures.TileDragCompleted += GridGesturesOnTileDragCompleted;
        _gridGestures.TileDragCancelled += GridGesturesOnTileDragCancelled;

        // And wire up the Drop Recognisers for the Dropping of a tile from 
        // the tile palette. 
        // ----------------------------------------------------------------
        var dropGesture = new DropGestureRecognizer();
        dropGesture.Drop += DropTileOnPanel;
        dropGesture.DragOver += DragOverTileOnPanel;
        dropGesture.DragLeave += DragLeaveTileOnPanel;
        dynamicGrid.GestureRecognizers.Add(dropGesture);
    }

    private async void GridGesturesOnSingleTap(object? sender, GridGestureEventArgs e) {
        try {
            if (DesignMode) {
                // Look up the tile if it is in this grid. If no tiles, reset selected
                // -------------------------------------------------------------------
                var tilesAtPosition = GridPositionHelper.GetTilesCovering(e.Col, e.Row, _dynamicGrid);
                if (tilesAtPosition.Count == 0) {
                    if (AppStateService.Instance.SelectedTile is {} tile && Panel is {} panel) {
                        var dropEntity = panel.CreateEntityFrom(tile.Entity);
                        dropEntity.Col = e.Col;
                        dropEntity.Row = e.Row;
                        panel.AddEntity(dropEntity);
                    } else {
                        ClearAllSelectedTiles();
                    }
                } else {
                    // Determine if any tile at this position is currently selected
                    // -------------------------------------------------------------------
                    var selectedIndexAtPos = tilesAtPosition.FindIndex(t => t.IsSelected);

                    // Case 1: None selected at this position -> unselect all tiles globally,
                    // then select the first at this position.
                    // -----------------------------------------------------------------------
                    if (selectedIndexAtPos == -1) {
                        MarkTileSelected(tilesAtPosition[0]);
                    }

                    // Case 2: A tile is selected at this position and there is only one tile -> unselect it.
                    // -------------------------------------------------------------------------------------
                    else if (tilesAtPosition.Count == 1) {
                        MarkTileUnSelected(tilesAtPosition[0]);
                    }

                    // Case 3: A tile is selected at this position and there are multiple tiles -> cycle selection.
                    // -------------------------------------------------------------------------------------------
                    else {
                        var nextIndex = (selectedIndexAtPos + 1) % tilesAtPosition.Count;
                        foreach (var t in tilesAtPosition.Where(t => t.IsSelected)) {
                            MarkTileUnSelected(t);
                        }

                        // Select the next tile in the stack
                        MarkTileSelected(tilesAtPosition[nextIndex]);
                    }
                }
                OnTileSelected(1);
            } else {
                var tile = GridPositionHelper.GetInteractiveTilesAt(e.Col, e.Row, _dynamicGrid).FirstOrDefault();
                if (tile is not null) OnTileTapped(tile, 1);
            }
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnSingleTap: {ex.Message}");
        }
    }

    private async void GridGesturesOnDoubleTap(object? sender, GridGestureEventArgs e) {
        try {
            if (DesignMode) {
                AppStateService.Instance.SelectedTile = null;
                var tile = GridPositionHelper.GetTilesCovering(e.Col, e.Row, _dynamicGrid).FirstOrDefault();

                // If we double-tapped on an actual tile, then unmark all tiles and mark this one
                // ------------------------------------------------------------------------------
                if (tile != null) {
                    ClearAllSelectedTiles();
                    MarkTileSelected(tile);
                    OnTileSelected(2);
                }

                // If there was no tile in this grid position, then use the Edit Mode to determine 
                // what we should do. Either move the tile to this position, or clone the selected tile
                // ------------------------------------------------------------------------------
                else {
                    await HandleDoubleTapEmptyCellMoveOrCopy(e);
                }
            } else {
                var tile = GridPositionHelper.GetInteractiveTilesAt(e.Col, e.Row, _dynamicGrid).FirstOrDefault();
                if (tile is not null) OnTileTapped(tile, 2);
            }
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnDoubleTap: {ex.Message}");
        }
    }

    private async void GridGesturesOnLongPress(object? sender, GridGestureEventArgs e) {
        try {
            if (DesignMode) {
                /* update here - if we support long press in design mode */
            } else {
                var tile = GridPositionHelper.GetTrackTilesAt(e.Col, e.Row, _dynamicGrid).FirstOrDefault();
                if (tile is TrackTile trackTile) await _pathTracer.StartPathTracing(trackTile);
            }
        } catch (Exception ex) {
            _logger.LogError($"Error in OnOperateModeLongPress: {ex.Message}");
        }
    }

    private async void GridGesturesOnTileDragStarted(object? sender, TileDragEventArgs e) {
        try {
            // You can't drag a Tile around if we are not in design mode
            // ---------------------------------------------------------
            if (!DesignMode) {
                _gridGestures?.CancelAllGestures();
                return;
            }
            
            AppStateService.Instance.SelectedTile = null;
            if (EditMode == EditModeEnum.Size && e.Tile?.Entity is not IDrawingEntity) {
                await ClickSounds.PlayError2SoundAsync();
                _gridGestures?.CancelAllGestures();
                return;
            }

            // If the tile is in the selected tiles list then we are moving ALL selected
            // tiles (or copying them). If it is not in the list, then we clear the list 
            // and add this tile as a selected tile. 
            // -------------------------------------------------------------------------
            if (e.Tile is { } tile) {
                if (!_selectedTiles.Contains(tile)) {
                    ClearAllSelectedTiles();
                    MarkTileSelected(tile);
                }
            }
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnTileDragStarted: {ex.Message}");
        }
    }

    private async void GridGesturesOnTileDragMoved(object? sender, TileDragEventArgs e) {
        try {
            switch (EditMode) {
            case EditModeEnum.Size when e.Tile is { Entity: IDrawingEntity } sizeTile:
                RemoveHighlights();
                ResizeTile(sizeTile, e.AbsStartCol, e.AbsStartRow, e.AbsEndCol, e.AbsEndRow);
                HighlightCell(sizeTile, CellHighlightAction.Resize);
                break;

            case EditModeEnum.Move or EditModeEnum.Copy when e.Tile is { } moveTile: {
                await HandleTileDragMoveOrCopy(e);
                break;
            }
            }
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnTileDragMoved: {ex.Message}");
        }
    }

    private async void GridGesturesOnTileDragCompleted(object? sender, TileDragEventArgs e) {
        try {
            RemoveHighlights();
            if (e.Tile is { } tile) {
                if (!GridPositionHelper.WouldCollide(tile, e.CurrentCol, e.CurrentRow, _dynamicGrid, EditMode) &&
                    GridPositionHelper.IsInBounds(tile, e.CurrentCol, e.CurrentRow, Cols, Rows)) {
                    switch (EditMode) {
                    case EditModeEnum.Move:
                        PerformMoveSelection(e.CurrentCol, e.CurrentRow);
                        break;

                    case EditModeEnum.Copy:
                        PerformCopySelection(e.CurrentCol, e.CurrentRow);
                        break;

                    case EditModeEnum.Size:
                        ResizeTile(tile, e.AbsStartCol, e.AbsStartRow, e.AbsEndCol, e.AbsEndRow);
                        MarkTileSelected(tile);
                        OnTileChanged(tile);
                        break;
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnTileDragCompleted: {ex.Message}");
        }
    }

    private async void GridGesturesOnTileDragCancelled(object? sender, TileDragEventArgs e) {
        try {
            RemoveHighlights();
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnTileDragCancelled: {ex.Message}");
        }
    }

    private async void GridGesturesOnGridSelectionStarted(object? sender, GridSelectionEventArgs e) {
        try {
            if (!DesignMode) _gridGestures?.CancelAllGestures();
            AppStateService.Instance.SelectedTile = null;
            ClearAllSelectedTiles();
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnGridSelectionStarted: {ex.Message}");
        }
    }

    private async void GridGesturesOnGridSelectionChanged(object? sender, GridSelectionEventArgs e) {
        try {
            UpdateSelectorView(e.AbsStartCol, e.AbsStartRow, e.AbsEndCol, e.AbsEndRow);
            MarkTilesSelectedInGrid(e.AbsStartCol, e.AbsStartRow, e.AbsEndCol, e.AbsEndRow);
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnGridSelectionChanged: {ex.Message}");
        }
    }

    private async void GridGesturesOnGridSelectionCompleted(object? sender, GridSelectionEventArgs e) {
        try {
            RemoveSelectorView();
            MarkTilesSelectedInGrid(e.AbsStartCol, e.AbsStartRow, e.AbsEndCol, e.AbsEndRow);
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnGridSelectionCompleted: {ex.Message}");
        }
    }

    private async void GridGesturesOnGridSelectionCancelled(object? sender, GridSelectionEventArgs e) {
        try {
            RemoveSelectorView();
            ClearAllSelectedTiles();
        } catch (Exception ex) {
            _logger.LogError($"Error in GridGesturesOnGridSelectionCancelled: {ex.Message}");
        }
    }
    #endregion

    #region Grid Management
    public Task ForceRefresh() => DrawPanel();

    /// <summary>
    /// Calculates the optimal grid size based on the specified width and height dimensions.
    /// </summary>
    public double CalculateGridSize(double width, double height) {
        if (width <= 0 || height <= 0) return 1;
        var gridSize = Math.Min(width / Cols, height / Rows);
        gridSize = Math.Floor(gridSize * 100) / 100.0;
        return gridSize;
    }

    /// <summary>
    /// Renders or refreshes the panel's grid based on the current state, dimensions, and the specified parameters.
    /// </summary>
    private async Task DrawPanel([CallerMemberName] string memberName = "",
                                 [CallerLineNumber] int sourceLineNumber = 0) {
        // Console.WriteLine($"DrawPanel: {memberName}@{sourceLineNumber} => Panel={(Panel == null ? "null" : "set")} IsDrawing={IsPanelDrawing} Size={MainGrid.Width}x{MainGrid.Height}");

        // Only redraw the grid if we absolutely need to. Events may mean that this 
        // is called multiple times, but if we really have not changed, then do not 
        // waste time redrawing and rebuilding the grid. 
        // -------------------------------------------------------------------------
        if (Panel is null || IsPanelDrawing) return;
        if (MainGrid.Width < 1.0 || MainGrid.Height < 1.0) return;

        // Draw the Grid. Make sure we clean up if it has already been drawn first
        // -------------------------------------------------------------------------
        using (new CodeTimer($"Draw Panel: {Panel?.Id} called from {memberName}@{sourceLineNumber}", ShowCodeTimer)) {
            MainThread.BeginInvokeOnMainThread(async void () => {
                try {
                    ControlPanelLayout.IsVisible = false;
                    IsPanelDrawing = true;

                    await Task.Yield();
                    await Task.Delay(10);

                    ClearAllSelectedTiles();
                    RemoveAllTilesFromGrid();

                    _gridSize = CalculateGridSize(MainGrid.Width, MainGrid.Height);
                    _viewWidth = _gridSize * Cols;
                    _viewHeight = _gridSize * Rows;
                    _overlayGridHighlights.CellSize = _gridSize;

                    _panelSurface.WidthRequest = _viewWidth;
                    _panelSurface.HeightRequest = _viewHeight;
                    _dynamicGrid.WidthRequest = _viewWidth;
                    _dynamicGrid.HeightRequest = _viewHeight;
                    _dynamicGrid.BackgroundColor = Panel?.PanelBackgroundColor ?? Colors.Transparent;

                    // If the grid rows and or columns have changed, then recreate the Dynamic Grid
                    // ------------------------------------------------------------------------------
                    _dynamicGrid.Children.Clear();

                    if (_dynamicGrid.RowDefinitions.Count != Rows || _dynamicGrid.ColumnDefinitions.Count != Cols) {
                        _dynamicGrid.RowDefinitions.Clear();
                        _dynamicGrid.ColumnDefinitions.Clear();

                        for (var i = 0; i < Rows; i++) {
                            _dynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                        }

                        for (var j = 0; j < Cols; j++) {
                            _dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                        }

                        _overlayGridLines.Update(Cols, Rows);
                    }

                    // Add all Entities to the Grid and Draw the Grid Lines
                    // ------------------------------------------------------------------------------
                    await AddEntitiesToGrid(Panel);
                } catch (Exception ex) {
                    _logger.LogError($"Error in DrawPanel: {ex.Message}");
                } finally {
                    IsPanelDrawing = false;
                    ControlPanelLayout.IsVisible = true;
                }
            });
        }
    }

    /// <summary>
    /// Given the Panel list of Entities, add each one as a tile to the panel.
    /// </summary>
    private async Task AddEntitiesToGrid(Panel? panel) {
        if (panel is null) return;
        _pathTracer.ClearTileRegistry();

        try {
            _dynamicGrid.BatchBegin();
            foreach (var entity in panel.Entities.OrderBy(x => x.Layer)) {
                AddEntityToGrid(entity);
            }
        } finally {
            _dynamicGrid.BatchCommit();
        }
    }

    /// <summary>
    /// Given an Entity, create a tile and add it to the panel grid.
    /// </summary>
    private ITile? AddEntityToGrid(Entity entity) {
        using (new CodeTimer($"Add Entity to Grid: {entity.EntityName}:{entity.Guid} @ {entity.Col},{entity.Row}", ShowCodeTimer, 10)) {
            var tile = TileFactory.CreateTile(entity, _gridSize, DesignMode ? TileDisplayMode.Design : TileDisplayMode.Normal);
            if (tile is not null) {
                tile.TileChanged += TileOnPropertiesChanged;
                if (tile is ContentView view) {
                    view.ClassId = entity.Guid.ToString();
                    view.ZIndex = entity.Layer;
                    SetTileGridPosition(tile);
                    _dynamicGrid.Children.Add(view);
                    if (tile is TrackTile trackTile) _pathTracer.RegisterTile(trackTile);
                }
                return tile;
            }
            _logger.LogError("Unable to create the tile for '{Entity}'", entity.EntityName);
            return null;
        }
    }

    /// <summary>
    /// Manage if any of the properties on the tile change and the tile needs to be redraw.
    /// </summary>
    private void TileOnPropertiesChanged(object? sender, TileChangedEventArgs e) {
        if (sender is Tile tile) {
            if (e.ChangeType == TileChangeType.Dimensions) {
                SetTileGridPosition(tile);
                tile.ForceRedraw();
            }
        }
    }

    /// <summary>
    /// This is a clean up route. If we redraw the grid, remove each tile from the grid first and ensure
    /// we have removed events and gestures.
    /// </summary>
    private void RemoveAllTilesFromGrid() {
        var children = _dynamicGrid.Children.OfType<ITile>().ToList();
        _dynamicGrid.BatchBegin();
        foreach (var tile in children) RemoveTileFromGrid(tile);
        _dynamicGrid.BatchCommit();
    }

    /// <summary>
    /// Remove an individual tile from the grid.
    /// </summary>
    private void RemoveTileFromGrid(ITile tile) {
        tile.TileChanged -= TileOnPropertiesChanged;
        RemoveEntityFromGrid(tile.Entity);
        (tile as IDisposable)?.Dispose();
        OnTileChanged(tile);
    }

    /// <summary>
    /// Remove an Entity from the Grid
    /// </summary>
    private void RemoveEntityFromGrid(Entity entity) {
        var views = _dynamicGrid.Children
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
            _dynamicGrid.Remove(view);
            if (view is TrackTile trackTile) _pathTracer.UnregisterTile(trackTile);
        }
    }

    private void ResizeTile(ITile? tile, int startCol, int startRow, int endCol, int endRow) {
        if (tile is null) return;

        tile.Entity.Col = startCol;
        tile.Entity.Row = startRow;
        tile.Entity.Width = endCol - startCol + 1;
        tile.Entity.Height = endRow - startRow + 1;

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
            _dynamicGrid.SetColumn(view, tile.Entity.Col);
            _dynamicGrid.SetRow(view, tile.Entity.Row);
            _dynamicGrid.SetColumnSpan(view, tile.Entity.Width);
            _dynamicGrid.SetRowSpan(view, tile.Entity.Height);
        }
    }
    #endregion

    #region Support Marking and UnMarking Tiles on the Panel
    /// <summary>
    /// Mark a tile as selected, and put a border around it
    /// </summary>
    public void MarkTileSelected(ITile tile) {
        _selectedTiles.Add(tile);
        HighlightCell(tile, CellHighlightAction.Selected);
        tile.IsSelected = true;
        OnTileSelected(0);
    }

    /// <summary>
    /// Unmark a tile, remove the border
    /// </summary>
    public void MarkTileUnSelected(ITile tile) {
        _selectedTiles.Remove(tile);
        UnHighlightCell(tile);
        tile.IsSelected = false;
        OnTileSelected(0);
    }

    public void MarkAllSelectedTiles() {
        RemoveHighlights();
        foreach (var tile in _selectedTiles) MarkTileSelected(tile);
    }

    /// <summary>
    /// Clear all tiles that are marked as selected
    /// </summary>
    public void ClearAllSelectedTiles() {
        foreach (var tile in _selectedTiles) MarkTileUnSelected(tile);
        RemoveHighlights();
        _selectedTiles.Clear();
        OnTileSelected(0);
    }

    // Highlight a given cell or location
    // ------------------------------------------------------------------------
    public void HighlightCell(ITile tile, CellHighlightAction action) => _overlayGridHighlights.Add(tile, action);
    public void HighlightCell(int col, int row, int width, int height, CellHighlightAction action) => _overlayGridHighlights.Add(col, row, width, height, action);

    // Given the start point of a highlighted cell, remove this cell highlight
    // ------------------------------------------------------------------------
    public void UnHighlightCell(ITile tile) => _overlayGridHighlights.Remove(tile);
    public void UnHighlightCell(int col, int row) => _overlayGridHighlights.Remove(col, row);

    // Clear all highlight views form the control panel
    // ------------------------------------------------------------------------
    public void RemoveHighlights() => _overlayGridHighlights.Clear();
    #endregion

    #region Support for the Grid Selector
    private void UpdateSelectorView(int startCol, int startRow, int endCol, int endRow) {
        _overlayGridSelection.Update(startCol, startRow, endCol, endRow, _gridSize);
        _overlayGridSelection.IsActive = true;
    }

    private void RemoveSelectorView() {
        _overlayGridSelection.IsActive = false;
    }

    private void MarkTilesSelectedInGrid(int startCol, int startRow, int endCol, int endRow) {
        // @formatter:off
        var unselectedTilesInRange = _dynamicGrid.Children
            .OfType<ITile>().Where(tile =>
                tile.Entity.Col >= startCol && tile.Entity.Col <= endCol &&
                tile.Entity.Row >= startRow && tile.Entity.Row <= endRow &&
                !tile.IsSelected)
                .ToList();

        var selectedTilesOutsideRange = _dynamicGrid.Children
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
    /// <summary>
    /// Entry point for a MOVE ot COPY operation on a Double-Click
    /// </summary>
    private async Task HandleDoubleTapEmptyCellMoveOrCopy(GridGestureEventArgs e) {
        var anchorCol = e.Col;
        var anchorRow = e.Row;

        // Must tap an empty cell to trigger move/copy by requirement
        // -----------------------------------------------------------
        if (GridPositionHelper.HasTileAt(anchorCol, anchorRow, _dynamicGrid)) return;
        if (_selectedTiles.Count == 0) return; // nothing to move/copy
        if (EditMode is not (EditModeEnum.Move or EditModeEnum.Copy)) return;

        var placeAt = CanPlaceSelectionAt(anchorCol, anchorRow, EditMode);
        if (!placeAt.isInBounds) {
            MainThread.BeginInvokeOnMainThread(async void () => {
                try {
                    RemoveHighlights();
                    foreach (var cell in placeAt.bounds) {
                        HighlightCell(cell.Rects.col, cell.Rects.row, cell.Rects.width, cell.Rects.height, cell.InBounds ? CellHighlightAction.Selected : CellHighlightAction.Error);
                    }
                    await Task.Yield();
                    await Task.Delay(150);
                    RemoveHighlights();
                    MarkAllSelectedTiles();
                } catch (Exception ex) {
                    _logger.LogError("Error marking tiles in Error: {Message}", ex.Message);
                }
            });
            return;
        }

        if (EditMode == EditModeEnum.Move) {
            PerformMoveSelection(anchorCol, anchorRow);
        } else {
            PerformCopySelection(anchorCol, anchorRow);
        }
    }

    private async Task HandleTileDragMoveOrCopy(TileDragEventArgs e) {
        var anchorCol = e.CurrentCol;
        var anchorRow = e.CurrentRow;

        var placeAt = CanPlaceSelectionAt(anchorCol, anchorRow, EditMode);
        MainThread.BeginInvokeOnMainThread(async void () => {
            try {
                RemoveHighlights();
                foreach (var cell in placeAt.bounds) {
                    HighlightCell(cell.Rects.col, cell.Rects.row, cell.Rects.width, cell.Rects.height, cell.InBounds ? CellHighlightAction.Selected : CellHighlightAction.Error);
                }
            } catch (Exception ex) {
                _logger.LogError("Error marking for move or copy tiles: {Message}", ex.Message);
            }
        });
    }

    /// <summary>
    /// Perform a MOVE operation on the selected tiles
    /// </summary>
    private void PerformMoveSelection(int anchorCol, int anchorRow) {
        if (_selectedTiles.Count == 0) return;
        var (minCol, minRow) = GetSelectionTopLeft()!.Value;

        // Move: update entity rows/cols in-place, then refresh grid positions
        // -------------------------------------------------------------------
        RemoveHighlights();
        var placeAt = CanPlaceSelectionAt(anchorCol, anchorRow, EditMode);
        if (placeAt.isInBounds) {
            foreach (var tile in _selectedTiles) {
                var e = tile.Entity;
                e.Col = anchorCol + (e.Col - minCol);
                e.Row = anchorRow + (e.Row - minRow);
                SetTileGridPosition(tile);
                tile.ForceRedraw();
                OnTileChanged(tile);
            }
        }
        MarkAllSelectedTiles();
    }

    /// <summary>
    /// Perform a COPY operation on the selected tiles
    /// </summary>
    private void PerformCopySelection(int anchorCol, int anchorRow) {
        if (_selectedTiles.Count == 0 || Panel is null) return;
        var (minCol, minRow) = GetSelectionTopLeft()!.Value;

        var placeAt = CanPlaceSelectionAt(anchorCol, anchorRow, EditMode);
        if (placeAt.isInBounds) {
            var tilesToCopy = _selectedTiles.ToList();
            foreach (var tile in tilesToCopy) {
                var e = tile.Entity;
                var newEntity = Panel.CreateEntityFrom(e);
                newEntity.Col = anchorCol + (e.Col - minCol);
                newEntity.Row = anchorRow + (e.Row - minRow);
                Panel.AddEntity(newEntity);
                OnTileChanged(tile);
            }

            ClearAllSelectedTiles();
            foreach (var tile in tilesToCopy) MarkTileSelected(tile);
        }
        MarkAllSelectedTiles();
    }

    /// <summary>
    /// Returns the top-left (min col,row) of the current selection.
    /// Returns null if no tiles are selected.
    /// </summary>
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

    private (bool isInBounds, List<DestinationBounds> bounds)
        CanPlaceSelectionAt(int anchorCol, int anchorRow, EditModeEnum mode) {
        if (_selectedTiles.Count == 0) return (true, new List<DestinationBounds>(0));

        var result = GridPositionHelper.EvaluateSelectionPlacement(
            selection: _selectedTiles,
            anchorCol: anchorCol,
            anchorRow: anchorRow,
            grid: _dynamicGrid,
            mode: mode,
            maxCols: Cols,
            maxRows: Rows);

        // Map helper’s PlacementRect -> existing DestinationBounds record
        var mapped = new List<DestinationBounds>(result.Cells.Count);
        foreach (var c in result.Cells) {
            mapped.Add(new DestinationBounds((c.Col, c.Row, c.Width, c.Height), c.InBounds && !c.Collides));
        }

        return (result.CanPlace, mapped);
    }

    private sealed record DestinationBounds((int col, int row, int width, int height) Rects, bool InBounds);
    #endregion

    #region Drag and Drop Support for the Tiles
    /// <summary>
    /// Called when we have left the bounds of the Panel so we just reset everything
    /// </summary>
    private void DragLeaveTileOnPanel(object? sender, DragEventArgs e) {
        if (!DesignMode) return;
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
            e.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Forbidden));
#endif
            return;
        }

        var tile = e.Data.Properties["Tile"] as ITile ?? null;
        if (tile is null) {
            Console.WriteLine("Got a Drop bt Tile is not set?");
            return;
        }
        
        var gridPosition = GridPositionHelper.GetGridPosition(e.GetPosition(_dynamicGrid), _dynamicGrid);
        if (gridPosition is { } position && (position.Col != _lastDragCol || position.Row != _lastDragRow)) {
            UnHighlightCell(_lastDragCol, _lastDragRow);
            if (!GridPositionHelper.WouldCollide(tile, position.Col, position.Row, _dynamicGrid, EditMode) &&
                GridPositionHelper.IsInBounds(tile, position.Col, position.Row, Cols, Rows)) {
                e.AcceptedOperation = DataPackageOperation.Copy;
                HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragValid);
            } else {
                e.AcceptedOperation = DataPackageOperation.None;
                HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragInvalid);
            }
            _lastDragCol = position.Col;
            _lastDragRow = position.Row;
        }

#if IOS || MACCATALYST
        e.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Copy));
#endif

#if WINDOWS
        var dragUI = e.PlatformArgs.DragEventArgs.DragUIOverride;
        dragUI.IsCaptionVisible = false;
        dragUI.IsGlyphVisible = false;
#endif
    }

    /// <summary>
    ///     Support dropping the dragged tile onto the panel in a new position (or the same position)
    /// </summary>
    private void DropTileOnPanel(object? sender, DropEventArgs e) {
        if (!DesignMode) return;
        try {
            ClearAllSelectedTiles();
            var tile = e.Data.Properties["Tile"] as ITile ?? null;
            if (tile is null) {
                Console.WriteLine("Got a Drop bt Tile is not set?");
                return;
            }
            if (Panel is { } panel) {
                var gridPosition = GridPositionHelper.GetGridPosition(e.GetPosition(_dynamicGrid), _dynamicGrid);
                if (gridPosition is { } position) {
                    // Make sure that the item we are placing is onto a point that is 
                    // not already occupied unless the item being dropped is an overlay 
                    // item that has a higher Z factor. 
                    // -----------------------------------------------------------------
                    if (!GridPositionHelper.WouldCollide(tile, position.Col, position.Row, _dynamicGrid, EditMode)) {
                        _logger.LogInformation("DropTileOnPanel: Mode=Symbol");
                        var dropEntity = panel.CreateEntityFrom(tile.Entity);
                        dropEntity.Col = position.Col;
                        dropEntity.Row = position.Row;
                        panel.AddEntity(dropEntity);
                        ClearAllSelectedTiles();
                        OnTileChanged(tile);
                    } else {
                        _logger.LogError("ERROR: Item clashes with existing track");
                    }
                } else {
                    _logger.LogError("ERROR: Invalid grid position");
                }
            }
        } catch (Exception ex) {
            _logger.LogError("ERROR: Error dropping item: {ExMessage} ", ex.Message);
        }
    }
    #endregion

    #region Bindable Properties
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty InteractiveProperty = BindableProperty.Create(nameof(Interactive), typeof(bool), typeof(ControlPanelView), true, BindingMode.Default);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty GridColorProperty = BindableProperty.Create(nameof(GridColor), typeof(Color), typeof(ControlPanelView), Colors.DarkGray, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty ShowTrackErrorsProperty = BindableProperty.Create(nameof(ShowTrackErrors), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default);
    public static readonly BindableProperty EditModeProperty = BindableProperty.Create(nameof(EditMode), typeof(EditModeEnum), typeof(ControlPanelView), EditModeEnum.Move, BindingMode.Default);

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
        try {
            if (bindable is ControlPanelView panel) {
                panel.ShowGrid = panel.DesignMode;
                panel._overlayGridHighlights.IsActive = panel.DesignMode;
                await panel.DrawPanel();
            }
        } catch (Exception ex) {
            Debug.WriteLine($"ERROR: OnDesignModeChanged: {ex.Message}");
        }
    }

    /// <summary>
    ///     If the Panel object is changed, then we need to clear and rebuild the whole Panel
    /// </summary>
    private static async void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        try {
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
        } catch (Exception e) {
            Debug.WriteLine($"ERROR: OnPanelChanged: {e.Message}");
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

    private static void OnShowGridChanged(BindableObject bindable, object oldvalue, object newvalue) {
        if (bindable is ControlPanelView control && newvalue is bool showGrid) {
            control._overlayGridLines.IsActive = showGrid;
        }
    }
    #endregion
}