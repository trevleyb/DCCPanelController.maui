using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services.SampleData;
using DCCPanelController.Tracks.Helpers;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View;

[ObservableObject]
public partial class ControlPanelView {

    public event EventHandler<ITrackPiece>? TrackPieceTapped;
    public event EventHandler<ITrackPiece>? TrackPieceChanged;

    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), null, BindingMode.OneTime, propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty ShowTrackErrorsProperty = BindableProperty.Create(nameof(ShowTrackErrors), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowTrackErrorsChanged);

    public EditModeEum EditMode = EditModeEum.Move;
   
    [ObservableProperty] private Color _gridColor = Colors.DarkGrey;
    [ObservableProperty] private double _gridSize;
    [ObservableProperty] private double _viewHeight;
    [ObservableProperty] private double _viewWidth;

    public ControlPanelView() {
        InitializeComponent();
        BindingContext = this;
        MainGrid.SizeChanged += OnGridSizeChanged;
    }

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

    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;

    public void Redraw() {
        RebuildGrid(true);
    }
    
    private void OnTrackPieceChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Track was changed: {e.PropertyName}");
        if (sender is ITrackPiece track) TrackPieceChanged?.Invoke(this,track);
    }

    private static void OnShowTrackErrorsChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        Console.WriteLine(bindable.GetType().Name + $" OnShowTrackErrorsChanged to {control.ShowTrackErrors}");
        control.RebuildGrid();
    }

    private static void OnShowGridChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        Console.WriteLine(bindable.GetType().Name + $" OnShowGridChanged to {control.ShowGrid}");
        control.AddOutlineToGrid();
    }

    private static void OnDesignModeChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        Console.WriteLine(bindable.GetType().Name + $" OnDesignModeChanged to {control.DesignMode}");
        if (control.DesignMode) {
            var dropRecogniser = new DropGestureRecognizer();
            dropRecogniser.Drop += control.DropTrackOnPanel;
            dropRecogniser.DragOver += control.DragOverTrackOnPanel;
            control.DynamicGrid.GestureRecognizers.Add(dropRecogniser);
        } else {
            control.DynamicGrid.GestureRecognizers.Clear();
        }
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        control.ClearSelectedTracks();
    }

    private void OnGridSizeChanged(object? sender, EventArgs e) {
        RebuildGrid();
    }

    public void RebuildGrid(bool forceRefresh = false) {
        if (Panel is null) return;
        if (MainGrid.Width < 1 || MainGrid.Height < 1) return;
        if (!forceRefresh && !HasScreenSizeChanged(MainGrid.Width, MainGrid.Height)) return;

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
    /// Only highlight a cell if we are in Design Mode
    /// </summary>
    public void HighlightCell(int col, int row) {
        if (!DesignMode) return;
        var border = new Border {
            BackgroundColor = Colors.Transparent,
            Stroke = Colors.Red,
            StrokeThickness = 4,
            Opacity = 0.5,
            ZIndex = 10,
            InputTransparent = true
        };

        // Add the Track Image to the appropriate grid position
        // ------------------------------------------------------
        DynamicGrid.SetRow(border, row);
        DynamicGrid.SetColumn(border, col);
        DynamicGrid.Children.Add(border);
    }

    /// <summary>
    /// Only UnHighlight a cell if we are operating in Design mode
    /// If we are in Operate mode, then we do not highlight cells so this has no function. 
    /// </summary>
    public void UnHighlightCell(int col, int row) {
        if (!DesignMode) return;
        var children = DynamicGrid.Children.Where(x => x is Border && x.Parent is Grid).ToList();
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

    /// <summary>
    ///     Add the tracks from the view model onto the Grid
    /// </summary>
    private void AddTrackPiecesToGrid() {
        Console.WriteLine("AddTrackPiecesToGrid()");
        if (Panel is { Tracks: { } tracks } panel) {
            foreach (var track in tracks) {

                if (DynamicGrid.ColumnDefinitions.Count >= Cols && DynamicGrid.RowDefinitions.Count >= Rows && track.X < Cols && track.Y < Rows) {
                    AddImageToLayout(track);

                    // If we need to overlay Valid/Invalid Options. Work out the points and draw error boxes
                    // -------------------------------------------------------------------------------------
                    if (ShowTrackErrors) {
                        var pointImage = new TrackPoints { X = track.X, Y = track.Y };
                        var validPoints = TrackPointsValidator.GetConnectedTracksStatus(tracks, track, panel.Cols, panel.Rows);
                        pointImage.SetPoints(validPoints);
                        AddImageToLayout(pointImage, true);
                    }
                    if (track.IsSelected) MarkTrackSelected(track);
                }
            }
        }
    }

    private void RemoveImageFromLayout(ITrackPiece track) {
        var tracksInGrid = DynamicGrid.Children;
    }

    private Image AddImageToLayout(ITrackPiece track, bool transparentInput = false) {
        var image = new Image {
            Scale = 1.5,
            ZIndex = track.Layer,
            Rotation = 0,
            InputTransparent = transparentInput,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };

        // Setup bindings to the size and source of the Track Image. Image can change on events
        // -------------------------------------------------------------------------------------------
        image.SetBinding(Image.SourceProperty, new Binding(nameof(track.Image)) { Source = track });
        image.SetBinding(RotationProperty, new Binding(nameof(track.ImageRotation)) { Source = track });
        image.SetBinding(WidthRequestProperty, new Binding(nameof(GridSize)) { Source = this });
        image.SetBinding(HeightRequestProperty, new Binding(nameof(GridSize)) { Source = this });

        // Setup trigger control to trap if we click on or select the track item
        // -------------------------------------------------------------------------------------------
        // Create TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (sender, args) => {
            Console.WriteLine($"Tapped on Track: {track?.Name ?? "Unknown or Invalid Track"}");
            Console.WriteLine($"There are {TrackPieceTapped?.GetInvocationList().Length ?? -1} listeners for TrackPieceTapped.");
            if (track != null) TrackPieceTapped?.Invoke(this, track);
        }; 
        //tapGesture.NumberOfTapsRequired = 1;
        image.GestureRecognizers.Add(tapGesture);

        // If we are in Design mode, then add support for 
        // dragging and dropping of the items on the page
        // ---------------------------------------------------------------------------------------
        if (DesignMode) {
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (sender, args) => DragTrackStarting(args, track);
            image.GestureRecognizers.Add(dragGesture);
        }

        // Add the Track Image to the appropriate grid position
        // ------------------------------------------------------
        DynamicGrid.SetRow(image, track.Y);
        DynamicGrid.SetColumn(image, track.X);
        DynamicGrid.Children.Add(image);
        track.PropertyChanged += OnTrackPieceChanged;
        return image;
    }

    // If we click on a grid that is NOT a track piece and in design mode, 
    // then clear all the selected tracks.
    // -------------------------------------------------------------------------
    private void TapGestureRecognizer_OnTapped(object? sender, TappedEventArgs e) {
        if (DesignMode) ClearSelectedTracks();
    }

    public void MarkTrackSelected(ITrackPiece track) {
        HighlightCell(track.X, track.Y);
        track.IsSelected = true;
    }

    public void MarkTrackUnSelected(ITrackPiece track) {
        UnHighlightCell(track.X,track.Y);
        track.IsSelected = false;
    }

    private void ClearSelectedTracks() {
        if (Panel is not null) {
            foreach (var track in Panel.SelectedTracks.Where(x => x.IsSelected)) MarkTrackUnSelected(track);
        }
    }

    private void DragTrackStarting(DragStartingEventArgs args, ITrackPiece track) {
        Console.WriteLine($"Dragging Track: {track.Name}");
        args.Data.Properties.Add("Track", track);
        args.Data.Properties.Add("Source", "Panel");
        args.Data.Properties.Add("Copy", EditMode == EditModeEum.Copy ? true: false);
    }

    private void DragOverTrackOnPanel(object? sender, DragEventArgs e) {
        var track = e.Data.Properties["Track"] as ITrackPiece ?? null;
        var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));

        e.AcceptedOperation = DataPackageOperation.None;
        if (gridPosition is { IsSuccess: true, Value: var position } && track != null) {
            if (track.Layer > GetHighestOccupiedLayer(position.Col, position.Row)) {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }
    }

    private void DropTrackOnPanel(object? sender, DropEventArgs e) {
        Console.WriteLine("Drop Gesture Recognizer OnDrop");
        try {
            if (!e.Data.Properties.ContainsKey("Source") ||
                !e.Data.Properties.ContainsKey("Track")) {
                Console.WriteLine("Could not determine the source of the item being dropped.");
                return;
            }
            
            var source = e.Data.Properties["Source"] as string ?? null;
            var track = e.Data.Properties["Track"] as ITrackPiece ?? null;
            var isCopyOperation = e.Data.Properties.ContainsKey("Copy") && (bool)e.Data.Properties["Copy"];
            var gridPosition = GetGridPosition(e?.GetPosition(DynamicGrid));
            
            if (gridPosition is { IsSuccess: true, Value: var position } && track is { } trackPiece) {
                // Make sure that the item we are placing is onto a point that is 
                // not already occupied unless the item being dropped is an overlay 
                // item that has a higher Z factor. 
                // -----------------------------------------------------------------
                if (trackPiece.Layer > GetHighestOccupiedLayer(position.Col, position.Row)) {
                    switch (source) {
                    case "Panel":
                        if (isCopyOperation) {
                            var newTrack = track.Clone(); 
                            trackPiece.X = position.Col;
                            trackPiece.Y = position.Row;
                            Panel?.Tracks?.Add(newTrack);
                            MarkTrackSelected(newTrack);
                        } else {
                            trackPiece.X = position.Col;
                            trackPiece.Y = position.Row;
                        }
                        RebuildGrid();
                        break;
                    case "Symbol":
                        if (Activator.CreateInstance(trackPiece.GetType()) is ITrackPiece newPiece) {
                            newPiece.X = position.Col;
                            newPiece.Y = position.Row;
                            Panel?.Tracks?.Add(newPiece);
                            ClearSelectedTracks();
                            MarkTrackSelected(newPiece);
                            OnTrackPieceChanged(newPiece, new PropertyChangedEventArgs(nameof(ITrackPiece)));
                        } else {
                            Console.WriteLine("Could not create a new Piece as a TrackPiece.");
                        }
                        RebuildGrid();
                        break;
                    default:
                        Console.WriteLine($"Invalid source: '{source}'");
                        break;
                    }
                } else {
                    Console.WriteLine("Grid location is already occupied.");
                }
            } else {
                Console.WriteLine($"Could not determine grid: {gridPosition.Error}");
            }
        } catch (Exception ex) {
            Console.WriteLine("Error dropping item: " + ex.Message);
        }
    }

    private int GetHighestOccupiedLayer(int col, int row) {
        var tracksInGrid = Panel?.Tracks.Where(x => x.X == col && x.Y == row).ToList() ?? new List<ITrackPiece>();
        if (tracksInGrid is {Count: > 0} ) return tracksInGrid.Max(track => (int?)track.Layer) ?? 0;
        return 0;
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

    public bool HasScreenSizeChanged(double width, double height) {
        const double tolerance = 5f;
        return Math.Abs(width - ViewWidth) > tolerance || Math.Abs(height - ViewHeight) > tolerance;
    }

    public void SetScreenSize(double width, double height) {
        GridSize = width > 0 && height > 0 ? Math.Min(width / Cols, height / Rows) / 2 * 2 : 1;
        ViewWidth = GridSize * Cols;
        ViewHeight = GridSize * Rows;
    }

    // public void HandleTrackPieceTapped(ITrackPiece track, int taps = 1) {
    //     TrackPieceTapped?.Invoke(this, track);
    //     if (DesignMode) {
    //         Console.WriteLine($"In design Mode: Handling {taps} for {track.Name}");
    //     } else {
    //         // if (track is ITrackInteractive) {
    //         //     switch (track) {
    //         //     case ITrackButton button:
    //                 Console.WriteLine($"You just tapped on {track.Name} - its a button so we will toggle it. ");
    //             //     button.Clicked();
    //             //     break;
    //             // case ITrackTurnout turnout:
    //             //     Console.WriteLine($"You just tapped on {track.Name} - its turnout so we will cycle states. ");
    //             //     turnout.Clicked();
    //             //     break;
    //             // }
    //         //}
    //     }
    // }
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
        Console.WriteLine("Drawing the Grid");
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