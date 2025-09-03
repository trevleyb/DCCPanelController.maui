using System.ComponentModel;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.ViewModel.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Helpers;

/// <summary>
/// Manages all gesture recognition and coordination for the grid panel.
/// Handles tap detection, long press, pointer events, and manual tile dragging.
/// </summary>
public class GridGestureHelper : IDisposable {
    private const int DoubleTapThreshold = 200;
    private const double DragSlopPx = 6;

    private readonly ILogger<GridGestureHelper> _logger;
    private readonly Grid _grid;
    private readonly object _tapLock = new();

    // Gesture state management
    public enum GestureOwner { None, Tap, LongPress, DragSelect }

    private GestureOwner _gestureOwner = GestureOwner.None;

    // Tap detection
    private int _tapCount;
    private Timer? _tapTimer;
    private DateTime _suppressTapsUntilUtc = DateTime.MinValue;
    private int _tappedCol;
    private int _tappedRow;

    // Long press state
    private bool _longPressActive;
    private bool _lpInvokedThisPress;

    // Manual tile drag state (for design mode)
    private bool _tileDragActive;
    private ITile? _draggedTile;
    private Point _tileDragStartPos;
    private int _dragStartCol;
    private int _dragStartRow;
    private int _lastDragCol;
    private int _lastDragRow;

    // Grid selection state
    private bool _dragSelectionActive;
    private Point? _pointerDownPos;

    // Gesture recognizers (kept as references for enable/disable)
    private TapGestureRecognizer? _gridTap;
    private TouchBehavior? _gridTouch;

    public GridGestureHelper(Grid grid, ILogger<GridGestureHelper> logger) {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SetupGestureRecognizers();
    }

    #region Events
    public event EventHandler<GridGestureEventArgs>? SingleTap;
    public event EventHandler<GridGestureEventArgs>? DoubleTap;
    public event EventHandler<GridGestureEventArgs>? LongPress;

    public event EventHandler<TileDragEventArgs>? TileDragStarted;
    public event EventHandler<TileDragEventArgs>? TileDragMoved;
    public event EventHandler<TileDragEventArgs>? TileDragCompleted;

    public event EventHandler<GridSelectionEventArgs>? GridSelectionStarted;
    public event EventHandler<GridSelectionEventArgs>? GridSelectionChanged;
    public event EventHandler<GridSelectionEventArgs>? GridSelectionCompleted;
    #endregion

    #region Public Properties
    public bool IsSelecting { get; private set; }
    public bool IsTileDragActive => _tileDragActive;
    public bool IsLongPressActive => _longPressActive;
    public GestureOwner CurrentGestureOwner => _gestureOwner;

    // Public properties to access stored tap position
    public int TappedCol => _tappedCol;
    public int TappedRow => _tappedRow;
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

    #region Gesture Event Handlers
    private void OnTapped(object? sender, TappedEventArgs e) {
        if (_longPressActive) return;
        if (TapsSuppressed()) return;
        if (_gestureOwner == GestureOwner.LongPress) return;
        if (_dragSelectionActive) return;

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

            switch (count) {
            case 1:
                SingleTap?.Invoke(this, gestureArgs);
                break;

            case 2:
                DoubleTap?.Invoke(this, gestureArgs);
                break;

            case >= 3:
                // Could add TripleTap event if needed
                break;
            }

            _gestureOwner = GestureOwner.None;
        });
    }

    private void OnLongPress(object? sender, LongPressCompletedEventArgs e) {
        try {
            if (_dragSelectionActive) return;
            if (_lpInvokedThisPress) return;

            _lpInvokedThisPress = true;
            _longPressActive = true;
            _gestureOwner = GestureOwner.LongPress;

            CancelTapTimer();
            SuppressTapsFor(500);

            // Use the stored tapped position since LongPressCompletedEventArgs doesn't have GetPosition()
            var gestureArgs = new GridGestureEventArgs(sender, _tappedCol, _tappedRow);
            LongPress?.Invoke(this, gestureArgs);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error in long press handler");
        }
    }
    #endregion

    #region Pointer Event Handlers (Manual Drag and Grid Selection)
    private void OnPointerPressed(object? sender, PointerEventArgs e) {
        _lpInvokedThisPress = false;
        _longPressActive = false;

        var cell = GridPositionHelper.GetGridPosition(e.GetPosition(_grid), _grid);
        if (cell is not { } gridCell) return;

        // Store the tapped position for long press events (which don't have position info)
        _tappedCol = gridCell.Col;
        _tappedRow = gridCell.Row;

        var tilesAtPosition = GridPositionHelper.GetTilesAt(gridCell.Col, gridCell.Row, _grid);

        // Check for potential tile drag in design mode
        if (tilesAtPosition.Count > 0) {
            _draggedTile = tilesAtPosition.FirstOrDefault();
            _tileDragStartPos = e.GetPosition(_grid) ?? new Point(0, 0);
            _tileDragActive = false; // Not active until movement threshold
            return;
        }

        // Start grid selection for empty cells
        _pointerDownPos = e.GetPosition(_grid);
        _dragSelectionActive = false;
        IsSelecting = true;

        GridSelectionStarted?.Invoke(this, new GridSelectionEventArgs(gridCell.Col, gridCell.Row));
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

                var dragArgs = new TileDragEventArgs(_draggedTile, _dragStartCol, _dragStartRow, _dragStartCol, _dragStartRow);
                TileDragStarted?.Invoke(this, dragArgs);
            }
        }

        // Handle active tile drag
        if (_tileDragActive && _draggedTile != null) {
            var gridPosition = GridPositionHelper.GetGridPosition(pos, _grid);
            if (gridPosition is { } position && (position.Col != _lastDragCol || position.Row != _lastDragRow)) {
                var dragArgs = new TileDragEventArgs(_draggedTile, _dragStartCol, _dragStartRow, position.Col, position.Row);
                TileDragMoved?.Invoke(this, dragArgs);
                _lastDragCol = position.Col;
                _lastDragRow = position.Row;
            }
            return;
        }

        // Handle grid selection
        if (!IsSelecting) return;

        // Check if we should activate drag selection
        if (!_dragSelectionActive && _pointerDownPos is { } startPos) {
            var dx = Math.Abs(pos.X - startPos.X);
            var dy = Math.Abs(pos.Y - startPos.Y);
            if (dx >= DragSlopPx || dy >= DragSlopPx) {
                _dragSelectionActive = true;
                _gestureOwner = GestureOwner.DragSelect;
                CancelTapTimer();
            }
        }

        // Update selection bounds
        var cell = GridPositionHelper.GetGridPosition(pos, _grid);
        if (cell is { } gridCell) {
            var selectionArgs = new GridSelectionEventArgs(_pointerDownPos, pos, gridCell.Col, gridCell.Row);
            GridSelectionChanged?.Invoke(this, selectionArgs);
        }
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e) {
        // Handle tile drag completion
        if (_tileDragActive && _draggedTile != null) {
            var gridPosition = GridPositionHelper.GetGridPosition(e.GetPosition(_grid), _grid);
            if (gridPosition is { } position) {
                var dragArgs = new TileDragEventArgs(_draggedTile, _dragStartCol, _dragStartRow, position.Col, position.Row);
                TileDragCompleted?.Invoke(this, dragArgs);
            }

            _tileDragActive = false;
            _draggedTile = null;
            _gestureOwner = GestureOwner.None;
            return;
        }

        // Handle grid selection completion
        if (IsSelecting) {
            var cell = GridPositionHelper.GetGridPosition(e.GetPosition(_grid), _grid);
            if (cell is { } gridCell) {
                var selectionArgs = new GridSelectionEventArgs(_pointerDownPos, e.GetPosition(_grid), gridCell.Col, gridCell.Row);
                GridSelectionCompleted?.Invoke(this, selectionArgs);
            }
        }

        ResetGestureState();
    }

    private void OnPointerExited(object? sender, PointerEventArgs e) {
        if (IsSelecting) {
            GridSelectionCompleted?.Invoke(this, new GridSelectionEventArgs(null, null, -1, -1));
        }

        ResetGestureState();
    }
    #endregion

    #region State Management
    private void ResetGestureState() {
        _gestureOwner = GestureOwner.None;
        _dragSelectionActive = false;
        _pointerDownPos = null;
        IsSelecting = false;
        _longPressActive = false;
        _lpInvokedThisPress = false;
        _tileDragActive = false;
        _draggedTile = null;
    }

    public void SuppressTapsFor(int milliseconds) {
        _suppressTapsUntilUtc = DateTime.UtcNow.AddMilliseconds(milliseconds);
    }

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
    /// Force cancel any active gestures and reset state
    /// </summary>
    public void CancelAllGestures() {
        CancelTapTimer();
        ResetGestureState();
        _gestureOwner = GestureOwner.None;
    }

    /// <summary>
    /// Enable or disable gesture recognition
    /// </summary>
    public void SetGesturesEnabled(bool enabled) {
        _gridTap?.SetValue(VisualElement.IsEnabledProperty, enabled);
        _gridTouch?.SetValue(VisualElement.IsEnabledProperty, enabled);
    }
    #endregion

    #region IDisposable
    public void Dispose() {
        CancelTapTimer();
        _grid.GestureRecognizers.Clear();
        _grid.Behaviors.Clear();
    }
    #endregion
}