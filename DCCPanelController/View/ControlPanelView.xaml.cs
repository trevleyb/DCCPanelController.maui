using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.Helpers;
using Microsoft.Maui.Layouts;
#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace DCCPanelController.View;

[ObservableObject]
public partial class ControlPanelView : IDisposable {
    public enum CellHighlightAction {
        Selected,
        DragInvalid,
        DragValid
    }

    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty ShowTrackErrorsProperty = BindableProperty.Create(nameof(ShowTrackErrors), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowTrackErrorsChanged);

    [ObservableProperty] private Color _gridColor = Colors.DarkGrey;
    [ObservableProperty] private double _gridSize;

    private int _lastDragCol;
    private int _lastDragRow;
    [ObservableProperty] private double _viewHeight;
    [ObservableProperty] private double _viewWidth;

    public EditModeEnum EditMode = EditModeEnum.Move;

    public ControlPanelView() {
        InitializeComponent();
        BindingContext = this;
        MainGrid.SizeChanged += OnGridSizeChanged;
    }

    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;

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

    public bool ShowTrackErrors {
        get => (bool)GetValue(ShowTrackErrorsProperty);
        set => SetValue(ShowTrackErrorsProperty, value);
    }

    /// <summary>
    ///     Clean up. We need to make sure all events are cleared and that
    ///     we reset the view to the image as this view has guestures attached
    ///     which cause other issues if not cleared. Clearing makes it regenerate.
    /// </summary>
    public void Dispose() {
        MainGrid.SizeChanged -= OnGridSizeChanged;

        if (Panel is { } panel) {
            try {
                foreach (var track in panel.Tracks) {
                    track.PropertyChanged -= OnTrackPieceChanged;
                    track.TrackViewRef = null;
                }
            } catch (Exception ex) {
                Debug.WriteLine($"ControlPanelView.Dispose Error: {ex.Message}");
            }
        }
    }

    public event EventHandler<ITrack>? TrackPieceTapped;
    public event EventHandler<ITrack>? TrackPieceChanged;
    public event EventHandler<ITrack>? TrackPieceDoubleTapped;

    private void OnTrackPieceChanged(object? sender, PropertyChangedEventArgs e) {
        if (sender is ITrack track) {
            if (e.PropertyName == nameof(track.TrackView)) {
                InvalidateCell(track);
            }

            TrackPieceChanged?.Invoke(this, track);
        }
    }

    private static void OnShowTrackErrorsChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        control.RebuildGrid();
    }

    private static void OnShowGridChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        control.AddOutlineToGrid();
    }

    private static void OnDesignModeChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;

        if (control.DesignMode) {
            var dropRecogniser = new DropGestureRecognizer();
            dropRecogniser.Drop += control.DropTrackOnPanel;
            dropRecogniser.DragOver += control.DragOverTrackOnPanel;
            dropRecogniser.DragLeave += control.DragLeaveTrackOnPanel;
            control.DynamicGrid.GestureRecognizers.Add(dropRecogniser);
        } else {
            control.DynamicGrid.GestureRecognizers.Clear();
        }
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        control.ClearSelectedTracks();
        control.RebuildGrid(true);
    }

    private void OnGridSizeChanged(object? sender, EventArgs e) {
        RebuildGrid();
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

    public void RebuildGrid(bool forceRefresh = false) {
        // Only redraw the grid if we absolutely need to. Events may mean that this 
        // is called multiple times, but if we really have not changed, then do not 
        // waste time redrawing and rebuilding the grid. 
        // -------------------------------------------------------------------------
        if (Panel is null) return;
        if (MainGrid.Width < 1.0 || MainGrid.Height < 1.0) return;
        if (!forceRefresh && !HasGridSizeChanged(MainGrid.Width, MainGrid.Height)) return;

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

        AddOutlineToGrid();
        AddTrackPiecesToGrid();
    }

    /// <summary>
    ///     Only highlight a cell if we are in Design Mode
    /// </summary>
    public void HighlightCell(int col, int row, int width, int height, CellHighlightAction action) {
        if (!DesignMode) return;

        var border = new Border {
            ClassId = "CellHighlight",
            BackgroundColor = Colors.Transparent,
            Stroke = action switch {
                CellHighlightAction.Selected    => Colors.Blue,
                CellHighlightAction.DragValid   => Colors.Green,
                CellHighlightAction.DragInvalid => Colors.Red,
                _                               => Colors.Red
            },
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            WidthRequest = width * GridSize,
            HeightRequest = height * GridSize,
            StrokeThickness = 4,
            Opacity = 0.5,
            ZIndex = 10,
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

    /// <summary>
    ///     Draw the Grid Outline
    /// </summary>
    private void RemoveOutlineFromGrid() {
        if (ControlPanelLayout.Children.Count >= 1) {
            var graphicsViewToRemove = ControlPanelLayout.Children.OfType<GraphicsView>().ToList();

            foreach (var view in graphicsViewToRemove) {
                ControlPanelLayout.Children.Remove(view);
            }
        }
    }

    private void AddOutlineToGrid() {
        RemoveOutlineFromGrid();

        if (ShowGrid) {
            var gridLines = new GridLinesDrawable(Rows, Cols);

            var graphicsView = new GraphicsView {
                InputTransparent = true,
                Drawable = gridLines,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            // Add the GraphicsView directly to the AbsoluteLayout
            AbsoluteLayout.SetLayoutBounds(graphicsView, new Rect(0.5, 0.5, ViewWidth, ViewHeight));
            AbsoluteLayout.SetLayoutFlags(graphicsView, AbsoluteLayoutFlags.PositionProportional);
            ControlPanelLayout.Children.Add(graphicsView);
            graphicsView.Invalidate();
        }
    }

    public void RemoveTrackPiece(ITrack track) {
        if (Panel is { Tracks: { } tracks } panel) {
            if (track.Parent == panel) {
                track.Parent = null;
                MarkTrackUnSelected(track);
                RemoveDisplayItemFromGrid(track);
                Panel.Tracks.Remove(track);
            }
        }
    }

    /// <summary>
    ///     Add the tracks from the view model onto the Grid
    /// </summary>
    private void AddTrackPiecesToGrid() {
        if (Panel is { Tracks: { } tracks } panel) {
            foreach (var track in tracks) {
                if (track.Parent != Panel) track.Parent = panel;

                if (DynamicGrid.ColumnDefinitions.Count >= Cols && DynamicGrid.RowDefinitions.Count >= Rows && track.X < Cols && track.Y < Rows) {
                    AddDisplayItemToGrid(track);

                    // If we need to overlay Valid/Invalid Options. Work out the points and draw error boxes
                    // -------------------------------------------------------------------------------------
                    if (ShowTrackErrors) {
                        var pointImage = new TrackPoints { X = track.X, Y = track.Y };
                        var validPoints = TrackPointsValidator.GetConnectedTracksStatus(tracks, track, panel.Cols, panel.Rows);
                        pointImage.SetPoints(validPoints);
                        AddDisplayItemToGrid(pointImage, true);
                    }

                    if (track.IsSelected) MarkTrackSelected(track);
                }
            }
        }
    }

    public void InvalidateCell(ITrack track) {
        RemoveDisplayItemFromGrid(track);
        AddDisplayItemToGrid(track);
    }

    private void RemoveDisplayItemFromGrid(ITrack track) {
        if (track.TrackViewRef is { } view) {
            DynamicGrid.Children.Remove(view);
            track.PropertyChanged -= OnTrackPieceChanged;
            track.TrackViewRef = null;
        }
    }

    private void AddDisplayItemToGrid(ITrack track, bool transparentInput = false) {
        var displayItem = track.TrackView(GridSize, transparentInput);
        track.PropertyChanged += OnTrackPieceChanged;

        // Setup trigger control to trap if we click on or select the track item
        // -------------------------------------------------------------------------------------------
        // Create TapGestureRecognizer
        if (displayItem is Microsoft.Maui.Controls.View view) {
            var tapGesture = new TapGestureRecognizer {
                NumberOfTapsRequired = 1
            };
            tapGesture.Tapped += (_, _) => TrackPieceTapped?.Invoke(this, track);
            view.GestureRecognizers.Add(tapGesture);

            var doubleTapGesture = new TapGestureRecognizer {
                NumberOfTapsRequired = 2
            };
            doubleTapGesture.Tapped += (sender, args) => TrackPieceDoubleTapped?.Invoke(this, track);
            view.GestureRecognizers.Add(doubleTapGesture);

            // If we are in Design mode, then add support for 
            // dragging and dropping of the items on the page
            // ---------------------------------------------------------------------------------------
            if (DesignMode) {
                var dragGesture = new DragGestureRecognizer();
                dragGesture.DragStarting += (sender, args) => DragTrackStarting(args, track);
                view.GestureRecognizers.Add(dragGesture);
            }
        }

        // Add the Track DisplayImage to the appropriate grid position
        // ------------------------------------------------------
        DynamicGrid.SetRow(displayItem, track.Y);
        DynamicGrid.SetColumn(displayItem, track.X);
        DynamicGrid.Children.Add(displayItem);
    }

    // If we click on a grid that is NOT a track piece and in design mode, 
    // then clear all the selected tracks.
    // -------------------------------------------------------------------------
    private void TapGestureRecognizer_OnTapped(object? sender, TappedEventArgs e) {
        if (DesignMode) ClearSelectedTracks();
    }

    public void MarkTrackSelected(ITrack track) {
        HighlightCell(track.X, track.Y, track.Width, track.Height, CellHighlightAction.Selected);
        track.IsSelected = true;
    }

    public void MarkTrackUnSelected(ITrack track) {
        UnHighlightCell(track.X, track.Y);
        track.IsSelected = false;
    }

    public void ClearSelectedTracks() {
        if (Panel is not null) {
            foreach (var track in Panel.SelectedTracks.Where(x => x.IsSelected)) MarkTrackUnSelected(track);
        }
    }

    private void DragTrackStarting(DragStartingEventArgs args, ITrack track) {
        args.Data.Properties.Add("Track", track);
        args.Data.Properties.Add("Source", "Panel");
        _lastDragCol = 0;
        _lastDragRow = 0;

#if IOS || MACCATALYST
        UIDragPreview Action() {
            var image = UIImage.FromFile("move.png");
            var imageView = new UIImageView(image);
            imageView.ContentMode = UIViewContentMode.Center;
            imageView.Frame = new CGRect(0, 0, GridSize, GridSize);
            return new UIDragPreview(imageView);
        }

        args?.PlatformArgs?.SetPreviewProvider(Action);
#endif
    }

    private void DragLeaveTrackOnPanel(object? sender, DragEventArgs e) {
        UnHighlightCell(_lastDragCol, _lastDragRow);
        _lastDragCol = 0;
        _lastDragRow = 0;
    }

    private void DragOverTrackOnPanel(object? sender, DragEventArgs e) {
        var track = e.Data.Properties["Track"] as ITrack ?? null;
        var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));

        if (gridPosition is { IsSuccess: true, Value: var position } && track != null) {
            if (EditMode == EditModeEnum.Size) {
                ResizeTrack(track, position.Col, position.Row);
            } else {
                if (_lastDragCol != position.Col || _lastDragRow != position.Row) {
                    UnHighlightCell(_lastDragCol, _lastDragRow);
                }

                //if (track.Layer > GetHighestOccupiedLayer(EditMode, track, position.Col, position.Row)) {
                if (!DoesTrackClash(track, position.Col, position.Row)) {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    HighlightCell(position.Col, position.Row, track.Width, track.Height, CellHighlightAction.DragValid);
                } else {
                    e.AcceptedOperation = DataPackageOperation.None;
                    HighlightCell(position.Col, position.Row, track.Width, track.Height, CellHighlightAction.DragInvalid);
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
        switch (EditMode) {
        case EditModeEnum.Move:
            e?.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Move));
            break;

        case EditModeEnum.Copy:
            e?.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Copy));
            break;

        default:
            e?.PlatformArgs?.SetDropProposal(new UIDropProposal(UIDropOperation.Forbidden));
            break;
        }
#endif

#if WINDOWS
    var dragUI = e.PlatformArgs.DragEventArgs.DragUIOverride;
    dragUI.IsCaptionVisible = false;
    dragUI.IsGlyphVisible = false;
#endif
    }

    private void DropTrackOnPanel(object? sender, DropEventArgs e) {
        try {
            if (!e.Data.Properties.ContainsKey("Source") ||
                !e.Data.Properties.ContainsKey("Track")) {
                return;
            }

            UnHighlightCell(_lastDragCol, _lastDragRow);
            var source = e.Data.Properties["Source"] as string ?? null;
            var track = e.Data.Properties["Track"] as ITrack ?? null;
            var gridPosition = GetGridPosition(e?.GetPosition(DynamicGrid));

            if (gridPosition is { IsSuccess: true, Value: var position } && track is { } trackPiece) {
                // Make sure that the item we are placing is onto a point that is 
                // not already occupied unless the item being dropped is an overlay 
                // item that has a higher Z factor. 
                // -----------------------------------------------------------------
                if (!DoesTrackClash(trackPiece, position.Col, position.Row)) {
                    ClearSelectedTracks();

                    if (Panel is { } panel) {
                        switch (source) {
                        case "Panel":
                            switch (EditMode) {
                            case EditModeEnum.Move:
                                RemoveDisplayItemFromGrid(trackPiece);
                                trackPiece.X = position.Col;
                                trackPiece.Y = position.Row;
                                AddDisplayItemToGrid(trackPiece);
                                break;

                            case EditModeEnum.Copy:
                                var newTrack = trackPiece.Clone(panel);
                                newTrack.X = position.Col;
                                newTrack.Y = position.Row;
                                panel.AddTrack(newTrack);
                                AddDisplayItemToGrid(newTrack);
                                MarkTrackSelected(newTrack);
                                break;

                            case EditModeEnum.Size:
                                break;
                            }

                            break;

                        case "DisplaySymbol":
                            if (Panel is not null && trackPiece.Clone(Panel) is { } newPiece) {
                                newPiece.X = position.Col;
                                newPiece.Y = position.Row;
                                Panel?.AddTrack(newPiece);
                                AddDisplayItemToGrid(newPiece);
                                MarkTrackSelected(newPiece);
                            }

                            break;

                        default:
                            Debug.WriteLine($"ERROR: Invalid source: '{source}'");
                            break;
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Debug.WriteLine("ERROR: Error dropping item: " + ex.Message);
        }

        _lastDragCol = 0;
        _lastDragRow = 0;
    }

    private bool DoesTrackClash(ITrack track, int col, int row) {
        if (Panel?.Tracks == null) return false;    // No clashes possible if no tracks are present
        if (track is not ITrackPiece) return false; // No clashes possible if the track is not a track piece

        var tracksInGrid = Panel.Tracks.Where(existingTrack =>

                                                  // Exclude the same track we're checking against
                                                  existingTrack != track && existingTrack is ITrackPiece &&

                                                  // Check if there's a column overlap between the tracks
                                                  col < existingTrack.X + existingTrack.Width && col + track.Width > existingTrack.X &&

                                                  // Check if there's a row overlap between the tracks
                                                  row < existingTrack.Y + existingTrack.Height && row + track.Height > existingTrack.Y
        );

        // If there are any tracks in the clashing list, return true
        return tracksInGrid.Any();
    }

    private int GetHighestOccupiedLayer(EditModeEnum editMode, ITrack track, int col, int row) {
        var tracksInGrid = Panel?.Tracks
                                 .Where(x =>
                                            (editMode == EditModeEnum.Copy || x != track) &&  // Include track if Copy mode, exclude if Move
                                            col < x.X + x.Width && col + track.Width > x.X && // Check X overlap, even if col is before
                                            row < x.Y + x.Height && row + track.Height > x.Y  // Check Y overlap, even if row is before
                                  )
                                 .ToList() ?? [];

        if (tracksInGrid is { Count: > 0 }) return tracksInGrid.Max(track => (int?)track.Layer) ?? 0;
        return 0;
    }

    private void ResizeTrack(ITrack? track, int newCol, int newRow) {
        if (track is null) return;

        // Original position and size
        var originalX = track.X;
        var originalY = track.Y;
        var originalWidth = track.Width;
        var originalHeight = track.Height;

        // Resizing right (increasing width)
        if (newCol > originalX) {
            track.Width = newCol - originalX + 1; // +1 to include the new column
        }

        // Resizing left (shifting X and adjusting width)
        else if (newCol < originalX) {
            var deltaX = originalX - newCol;
            track.X -= deltaX;     // Shift left
            track.Width += deltaX; // Increase width
        }

        // Resizing down (increasing height)
        if (newRow > originalY) {
            track.Height = newRow - originalY + 1; // +1 to include the new row
        }

        // Resizing up (shifting Y and adjusting height)
        else if (newRow < originalY) {
            var deltaY = originalY - newRow;
            track.Y -= deltaY;      // Shift up
            track.Height += deltaY; // Increase height
        }

        // Ensure minimum size limits
        track.Width = Math.Max(1, track.Width);
        track.Height = Math.Max(1, track.Height);

        // Refresh Grid and Update
        InvalidateCell(track); // Re-render the grid for the resized component
    }

    /// <summary>
    ///     Convert a position in the grid (absolute) to a Grid position within the col/row definitions
    /// </summary>
    /// <param name="point">A point object of where the item was tapped</param>
    /// <returns>Either a null, or (-1,-1) or (row,col) </returns>
    private Result<(int Col, int Row)> GetGridPosition(Point? point) {
        if (point is { } tapPosition) {
            var totalHeight = DynamicGrid.Height;
            var totalWidth = DynamicGrid.Width;
            var rowCount = DynamicGrid.RowDefinitions.Count;
            var colCount = DynamicGrid.ColumnDefinitions.Count;

            var cellHeight = totalHeight / rowCount;
            var cellWidth = totalWidth / colCount;

            if (cellHeight == 0 || cellWidth == 0) {
                return Result<(int Col, int Row)>.Failure("Cell Width or Height is zero.");
            }

            // Calculate row and column indices
            var row = (int)(tapPosition.Y / cellHeight);
            var col = (int)(tapPosition.X / cellWidth);

            // Ensure indices are within bounds
            row = Math.Min(row, rowCount - 1);
            col = Math.Min(col, colCount - 1);

            return Result<(int Col, int Row)>.Success((col, row));
        }

        return Result<(int Col, int Row)>.Failure("Could not determine the Grid Position from the point provided,") ?? throw new InvalidOperationException();
    }
}

public enum EditModeEnum {
    Move,
    Copy,
    Size
}

/// <summary>
///     This is a helper class that draws the Grid Lines on the Page.
/// </summary>
/// <param name="rows">Number of rows to Draw</param>
/// <param name="columns">Number of cols to Draw</param>
internal class GridLinesDrawable(int rows, int columns, Color? gridColor = null, float? lineWidth = null, float? gridWidth = null) : IDrawable {
    private Color GridColor { get; } = gridColor ?? Colors.DarkGrey;
    private float LineWidth { get; } = lineWidth ?? 0.5f;
    private float GridWidth { get; } = gridWidth ?? 5.0f;

    public void Draw(ICanvas canvas, RectF dirtyRect) {
        var cellWidth = dirtyRect.Width / columns;
        var cellHeight = dirtyRect.Height / rows;
        canvas.StrokeColor = GridColor;

        for (var i = 0; i <= rows; i++) {
            canvas.StrokeSize = i == 0 || i == rows ? GridWidth : LineWidth;
            canvas.DrawLine(0, i * cellHeight, dirtyRect.Width, i * cellHeight);
        }

        for (var j = 0; j <= columns; j++) {
            canvas.StrokeSize = j == 0 || j == columns ? GridWidth : LineWidth;
            canvas.DrawLine(j * cellWidth, 0, j * cellWidth, dirtyRect.Height);
        }
    }
}