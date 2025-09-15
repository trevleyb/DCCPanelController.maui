using System.Diagnostics;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.View.ControlPanel;

/// <summary>
///     Manages all gesture recognition and coordination for the grid panel.
///     Handles tap detection, long press, pointer events, and manual tile dragging.
/// </summary>
public class GridGestureHelper : IDisposable {
    // Gesture state management
    public enum GestureOwner { None, Tap, LongPress, DragSelect }
    public bool EnableDoubleTap { get; set; } = true;

    private const int    DoubleTapThreshold = 225;
    private const double DragSlopPx         = 3.5;

    // Debouce Support
    private const int MoveMinIntervalMs = 10; // try 8–12

    private readonly Grid   _grid;
    private readonly object _tapLock = new();

    // Tile or element being dragged
    private ITile? _draggedTile;
    private int    _dragStartCol;
    private int    _dragStartRow;

    private GestureOwner _gestureOwner = GestureOwner.None;

    // Gesture recognizers (kept as references for enable/disable)
    private TapGestureRecognizer? _gridTap;
    private TouchBehavior?        _gridTouch;
    private int                   _lastDragCol;
    private int                   _lastDragRow;
    private int                   _lastMoveCol;
    private long                  _lastMoveProcessedMs;
    private int                   _lastMoveRow;
    private int                   _lastProcessedCol = -1;
    private int                   _lastProcessedRow = -1;

    // Long press state
    private bool  _longPressActive;
    private bool  _longPressDetected; // Timer detected long press duration
    private Point _longPressStartPos;
    private bool  _lpInvokedThisPress;

    // Grid selection state
    private Point?   _pointerDownPos;
    private int      _selectionStartCol;
    private int      _selectionStartRow;
    private DateTime _suppressTapsUntilUtc = DateTime.MinValue;

    // Tap detection
    private int    _tapCount;
    private int    _tappedCol;
    private int    _tappedRow;
    private Timer? _tapTimer;
    private bool   _tileDragActive;

    // Manual tile drag state (for design mode)
    private Point _tileDragStartPos;

    public GridGestureHelper(Grid grid) {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        SetupGestureRecognizers();
    }

    #region Public Properties
    public bool IsSelecting { get; private set; }
    #endregion

    #region IDisposable
    public void Dispose() {
        CancelTapTimer();
        _grid.GestureRecognizers.Clear();
        _grid.Behaviors.Clear();
    }
    #endregion

    #region Setup and Configuration
    private void SetupGestureRecognizers() {
        _grid.GestureRecognizers.Clear();
        _grid.Behaviors.Clear();

        // Pointer events for manual dragging and selection
        var pointerRecognizer = new PointerGestureRecognizer();
        pointerRecognizer.PointerPressed += OnPointerPressed;
        pointerRecognizer.PointerMoved += OnPointerMoved;
        pointerRecognizer.PointerReleased += OnPointerReleased;
        pointerRecognizer.PointerExited += OnPointerExited;
        _grid.GestureRecognizers.Add(pointerRecognizer);

        // Tap recognition
        _gridTap = new TapGestureRecognizer();
        _gridTap.Tapped += OnTapped;
        _grid.GestureRecognizers.Add(_gridTap);

        // Long press recognition
        _gridTouch = new TouchBehavior();
        _gridTouch.LongPressCompleted += OnLongPress;
        _grid.Behaviors.Add(_gridTouch);
    }
    #endregion

    #region Events
    public event EventHandler<GridGestureEventArgs>? SingleTap;
    public event EventHandler<GridGestureEventArgs>? DoubleTap;
    public event EventHandler<GridGestureEventArgs>? LongPress;

    public event EventHandler<TileDragEventArgs>? TileDragStarted;
    public event EventHandler<TileDragEventArgs>? TileDragMoved;
    public event EventHandler<TileDragEventArgs>? TileDragCompleted;
    public event EventHandler<TileDragEventArgs>? TileDragCancelled;

    public event EventHandler<GridSelectionEventArgs>? GridSelectionStarted;
    public event EventHandler<GridSelectionEventArgs>? GridSelectionChanged;
    public event EventHandler<GridSelectionEventArgs>? GridSelectionCompleted;
    public event EventHandler<GridSelectionEventArgs>? GridSelectionCancelled;
    #endregion

    #region Gesture Event Handlers
    private void OnTapped(object? sender, TappedEventArgs e) {
        if (_longPressActive) return;
        if (TapsSuppressed()) return;
        if (_gestureOwner == GestureOwner.LongPress) return;
        if (IsSelecting) return;

        lock (_tapLock) {
            var pos = GridPositionHelper.GetGridPosition(e.GetPosition(_grid), _grid) ?? (-1, -1);
            _tapCount++;
            _gestureOwner = GestureOwner.Tap;
            _tapTimer?.Dispose();
            _tapTimer = new Timer(OnTapTimerElapsed,
                new GridGestureEventArgs(sender, pos.Col, pos.Row),
                DoubleTapThreshold,
                Timeout.Infinite);
        }
    }

    private void OnTapTimerElapsed(object? state) {
        if (state is not GridGestureEventArgs gestureArgs) return;

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
            gestureArgs.TapCount = count;
            if (!EnableDoubleTap) {
                SingleTap?.Invoke(this, gestureArgs);
            } else {
                switch (count) {
                    case 1:
                        SingleTap?.Invoke(this, gestureArgs);
                    break;

                    case 2:
                        DoubleTap?.Invoke(this, gestureArgs);
                    break;

                    case>= 3:
                        // Could add TripleTap event if needed
                    break;
                }
            }
            _gestureOwner = GestureOwner.None;
        });
    }

    private async void OnLongPress(object? sender, LongPressCompletedEventArgs e) {
        try {
            if (IsSelecting || _tileDragActive) return;
            if (_lpInvokedThisPress) return;
            _longPressDetected = true;
        } catch (Exception ex) {
            Debug.WriteLine("Error in long press handler: " + ex.Message);
        }
    }
    #endregion

    #region Pointer Event Handlers
    private void OnPointerPressed(object? sender, PointerEventArgs e) {
        _lpInvokedThisPress = false;
        _longPressActive = false;
        _longPressDetected = false;

        var cell = GridPositionHelper.GetGridPosition(e.GetPosition(_grid), _grid);
        if (cell is not { } gridCell) return;

        // Store the tapped position for long press events (which don't have position info)
        _tappedCol = gridCell.Col;
        _tappedRow = gridCell.Row;
        _longPressStartPos = e.GetPosition(_grid) ?? new Point(0, 0);

        //var tilesAtPosition = GridPositionHelper.GetTilesAt(gridCell.Col, gridCell.Row, _grid);
        var tilesAtPosition = GridPositionHelper.GetTilesCovering(gridCell.Col, gridCell.Row, _grid);

        // Check for potential tile drag in design mode
        if (tilesAtPosition.Count > 0) {
            _draggedTile = tilesAtPosition.FirstOrDefault();
            _tileDragStartPos = e.GetPosition(_grid) ?? new Point(0, 0);
            _tileDragActive = false; // Not active until movement threshold
            return;
        }

        // Prepare for potential grid selection on empty cells - but don't start yet
        _pointerDownPos = e.GetPosition(_grid);
        _selectionStartCol = gridCell.Col;
        _selectionStartRow = gridCell.Row;
        IsSelecting = false;

        // Note: IsSelecting is now based on _dragSelectionActive, so it remains false here
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e) {
        var currentPos = e.GetPosition(_grid);
        if (currentPos is not { } pos) return;

        // Handle potential tile drag activation
        if (_draggedTile != null && !_tileDragActive) {
            var dx = Math.Abs(pos.X - _tileDragStartPos.X);
            var dy = Math.Abs(pos.Y - _tileDragStartPos.Y);

            if (dx >= DragSlopPx || dy >= DragSlopPx) {
                _tileDragActive = true;
                _gestureOwner = GestureOwner.DragSelect;
                CancelTapTimer();

                _dragStartCol = _draggedTile.Entity.Col;
                _dragStartRow = _draggedTile.Entity.Row;
                _lastDragCol = _dragStartCol;
                _lastDragRow = _dragStartRow;

                var dragArgs = new TileDragEventArgs(_draggedTile, _dragStartCol, _dragStartRow);
                TileDragStarted?.Invoke(this, dragArgs);
            }
        }

        var gridPosition = GridPositionHelper.GetGridPosition(pos, _grid);

        var nowMs = Environment.TickCount64;
        var cellChanged = gridPosition?.Col != _lastProcessedCol || gridPosition?.Row != _lastProcessedRow;
        if (!cellChanged && nowMs - _lastMoveProcessedMs < MoveMinIntervalMs) return;

        _lastMoveProcessedMs = nowMs;
        _lastProcessedCol = gridPosition?.Col ?? -1;
        _lastProcessedRow = gridPosition?.Row ?? -1;

        // Handle active tile drag
        if (_tileDragActive && _draggedTile != null) {
            if (gridPosition is { } position && (position.Col != _lastDragCol || position.Row != _lastDragRow)) {
                var dragArgs = new TileDragEventArgs(_draggedTile, _dragStartCol, _dragStartRow, position.Col, position.Row, _lastDragCol, _lastDragRow);
                TileDragMoved?.Invoke(this, dragArgs);
                _lastDragCol = position.Col;
                _lastDragRow = position.Row;
            }
            return;
        }

        // Handle potential grid selection activation
        if (_pointerDownPos is { } startPos && !IsSelecting) {
            var dx = Math.Abs(pos.X - startPos.X);
            var dy = Math.Abs(pos.Y - startPos.Y);

            if (dx >= DragSlopPx || dy >= DragSlopPx) {
                IsSelecting = true;
                _gestureOwner = GestureOwner.DragSelect;
                CancelTapTimer();

                // Clear long press detection since we're now dragging
                _longPressDetected = false;

                // NOW fire the GridSelectionStarted event
                GridSelectionStarted?.Invoke(this, new GridSelectionEventArgs(_selectionStartCol, _selectionStartRow));
            }
        }

        // Handle active grid selection
        if (IsSelecting) {
            if (gridPosition is { } gridCell) {
                if (gridCell.Col != _lastMoveCol || gridCell.Row != _lastMoveRow) {
                    var selectionArgs = new GridSelectionEventArgs(_selectionStartCol, _selectionStartRow, gridCell.Col, gridCell.Row);
                    GridSelectionChanged?.Invoke(this, selectionArgs);
                    _lastMoveCol = gridCell.Col;
                    _lastMoveRow = gridCell.Row;
                }
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e) {
        var currentPos = e.GetPosition(_grid) ?? new Point(0, 0);

        _lastMoveProcessedMs = 0;
        _lastProcessedCol = -1;
        _lastProcessedRow = -1;

        // Handle tile drag completion
        if (_tileDragActive && _draggedTile != null) {
            var gridPosition = GridPositionHelper.GetGridPosition(e.GetPosition(_grid), _grid);
            if (gridPosition is { } position) {
                var dragArgs = new TileDragEventArgs(_draggedTile, _dragStartCol, _dragStartRow, position.Col, position.Row, _lastDragCol, _lastDragRow);
                TileDragCompleted?.Invoke(this, dragArgs);
            }

            _tileDragActive = false;
            _draggedTile = null;
            _gestureOwner = GestureOwner.None;
            return;
        }

        // Handle grid selection completion - only if selection was actually active
        if (IsSelecting) {
            var cell = GridPositionHelper.GetGridPosition(e.GetPosition(_grid), _grid);
            if (cell is { } gridCell) {
                var selectionArgs = new GridSelectionEventArgs(_selectionStartCol, _selectionStartRow, gridCell.Col, gridCell.Row);
                GridSelectionCompleted?.Invoke(this, selectionArgs);
            }
        }

        // Check for long press on release (if timer detected it and pointer didn't move much)
        else if (_longPressDetected && !_lpInvokedThisPress) {
            var dx = Math.Abs(currentPos.X - _longPressStartPos.X);
            var dy = Math.Abs(currentPos.Y - _longPressStartPos.Y);

            if (dx < DragSlopPx && dy < DragSlopPx) {
                _lpInvokedThisPress = true;
                _longPressActive = true;
                _gestureOwner = GestureOwner.LongPress;

                CancelTapTimer();
                SuppressTapsFor(500);

                var gestureArgs = new GridGestureEventArgs(sender, _tappedCol, _tappedRow);
                LongPress?.Invoke(this, gestureArgs);
            }
        }
        ResetGestureState();
    }

    private void OnPointerExited(object? sender, PointerEventArgs e) {
        _lastMoveProcessedMs = 0;
        _lastProcessedCol = -1;
        _lastProcessedRow = -1;

        if (IsSelecting) {
            GridSelectionCancelled?.Invoke(this, new GridSelectionEventArgs(_selectionStartCol, _selectionStartRow, -1, -1));
        }

        if (_tileDragActive) {
            var dragArgs = new TileDragEventArgs(_draggedTile, _dragStartCol, _dragStartRow, -1, -1, _lastDragCol, _lastDragRow);
            TileDragCancelled?.Invoke(this, dragArgs);
        }
        ResetGestureState();
    }
    #endregion

    #region State Management
    public void CancelCurrentGesture() => ResetGestureState();

    private void ResetGestureState() {
        _gestureOwner = GestureOwner.None;
        IsSelecting = false;
        _pointerDownPos = null;
        _longPressActive = false;
        _lpInvokedThisPress = false;
        _longPressDetected = false;
        _tileDragActive = false;
        _draggedTile = null;
    }

    public void SuppressTapsFor(int milliseconds) => _suppressTapsUntilUtc = DateTime.UtcNow.AddMilliseconds(milliseconds);

    private void CancelTapTimer() {
        lock (_tapLock) {
            _tapCount = 0;
            _tapTimer?.Dispose();
            _tapTimer = null;
        }
    }

    private bool TapsSuppressed() => DateTime.UtcNow < _suppressTapsUntilUtc;
    #endregion

    #region Public Methods
    /// <summary>
    ///     Force cancel any active gestures and reset state
    /// </summary>
    public void CancelAllGestures() {
        CancelTapTimer();
        ResetGestureState();
        _gestureOwner = GestureOwner.None;
    }

    /// <summary>
    ///     Enable or disable gesture recognition
    /// </summary>
    public void SetGesturesEnabled(bool enabled) {
        _gridTap?.SetValue(VisualElement.IsEnabledProperty, enabled);
        _gridTouch?.SetValue(VisualElement.IsEnabledProperty, enabled);
    }
    #endregion
}