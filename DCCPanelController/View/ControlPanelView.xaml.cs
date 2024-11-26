using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Events;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Helpers;
using Microsoft.Maui.Layouts;
using System.Timers;
using DCCPanelController.Tracks.Interfaces;

//
// This is a COMPONENT that is used inside the operate and panels views
//
namespace DCCPanelController.View;

[ObservableObject]
public partial class ControlPanelView {

    private readonly System.Timers.Timer _tapTimer;
    private ITrackPiece? _selectedTrack;
    private int _tapCount;
    
    public event EventHandler<TrackSelectedEvent>? TrackTapped;
    public event EventHandler<TrackSelectedEvent>? TrackPieceTapped;
   
    public ControlPanelView() {
        BindingContext = this;
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        MainGrid.SizeChanged += OnGridSizeChanged;

        _tapTimer = new System.Timers.Timer {
            Interval = 300,   // Adjust as needed (300ms works well for distinguishing between single and double taps)
            AutoReset = false // Make sure it does not repeat automatically
        };

        _tapTimer.Elapsed += OnTapTimerElapsed;
    }

    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), null, BindingMode.OneTime, propertyChanged: OnPanelChanged);
    public Panel? Panel {
        get => (Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }
    
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public bool DesignMode {
        get => (bool)GetValue(DesignModeProperty);
        set => SetValue(DesignModeProperty, value);
    }
    
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public bool ShowGrid {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }
    
    public static readonly BindableProperty ShowTrackErrorsProperty = BindableProperty.Create(nameof(ShowTrackErrors), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowTrackErrorsChanged);
    public bool ShowTrackErrors {
        get => (bool)GetValue(ShowTrackErrorsProperty);
        set => SetValue(ShowTrackErrorsProperty, value);
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
            control.DynamicGrid.GestureRecognizers.Add(dropRecogniser);
        } else {
            control.DynamicGrid.GestureRecognizers.Clear();
        }
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        Console.WriteLine(bindable.GetType().Name + $" OnPanelChanged to {control.Panel?.Name}");
    }

    [ObservableProperty] private double _viewWidth;
    [ObservableProperty] private double _viewHeight;
    [ObservableProperty] private double _gridSize;
    [ObservableProperty] private Color _gridColor = Colors.DarkGrey;

    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        //Console.WriteLine($"PropertyChanged: {e.PropertyName}");
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

        if (DesignMode) AddOutlineToGrid();
        AddTrackPiecesToGrid();
    }

    public void HighlightCell(int col, int row) {
        Console.WriteLine($"HighlightCell({col},{row})");
        var border = new Border() {
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

    public void UnHighlightCell(int col, int row) {
        Console.WriteLine($"UnHighlightCell({col},{row})");
        var children = DynamicGrid.Children.Where(x => x is Border && x.Parent is Grid).ToList();
        foreach (var child in children) {
            if (DynamicGrid.GetRow(child) == row && DynamicGrid.GetColumn(child) == col) {
                DynamicGrid.Remove(child);
            }
        }
    }

    /// <summary>
    /// Draw the Grid Outline
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
            var gridLines = new GridLinesDrawable(Rows,Cols);
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
    /// Add the tracks from the view model onto the Grid
    /// </summary>
    private void AddTrackPiecesToGrid() {
        Console.WriteLine($"AddTrackPiecesToGrid()");
        if (Panel is { Tracks: { } tracks } panel ) {
            foreach (var track in tracks) {
                if (DynamicGrid.ColumnDefinitions.Count >= Cols && DynamicGrid.RowDefinitions.Count >= Rows && track.X < Cols && track.Y < Rows) {
                    var image = AddImageToLayout(track);
                }

                // If we need to overlay Valid/Invalid Options. Work out the points and draw error boxes
                // -------------------------------------------------------------------------------------
                if (DesignMode && ShowTrackErrors) {
                    var pointImage = new TrackPoints { X = track.X, Y = track.Y };
                    var validPoints = TrackPointsValidator.GetConnectedTracksStatus(tracks, track, panel.Cols, panel.Rows);
                    pointImage.SetPoints(validPoints);
                    var image = AddImageToLayout(pointImage);
                    image.InputTransparent = true;
                }
            }
        }
    }

    private void RemoveImageFromLayout(ITrackPiece track) {
        var tracksInGrid = DynamicGrid.Children;
    }

    private Image AddImageToLayout(ITrackPiece track) {
        var image = new Image {
            Scale = 1.5,
            ZIndex = track.Layer,
            Rotation = 0,
            InputTransparent = false,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };

        // Setup bindings to the size and source of the Track Image. Image can change on events
        // -------------------------------------------------------------------------------------------
        image.SetBinding(Image.SourceProperty, new Binding(nameof(track.Image)) { Source = track });
        image.SetBinding(RotationProperty, new Binding(nameof(track.ImageRotation)) { Source = track });
        image.SetBinding(WidthRequestProperty, new Binding(nameof(GridSize)) { Source = this});
        image.SetBinding(HeightRequestProperty, new Binding(nameof(GridSize)) { Source = this });

        // Setup trigger control to trap if we click on or select the track item
        // -------------------------------------------------------------------------------------------
        // Create TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => OnTrackPieceTapped(track);
        tapGesture.NumberOfTapsRequired = 1;
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
        return image;
    }

    private void OnTrackPieceTapped(ITrackPiece track) {
        Console.WriteLine($"Track Piece Tapped: {track.Name} with tapCount={_tapCount}");
        _selectedTrack = track;
        _tapCount++; // Increment the tap count whenever a tap is detected
        Console.WriteLine($"Tapped Event with TapCount now = {_tapCount}");

        if (_tapCount == 1) {
            // Start or reset the timer when the first tap is detected
            _tapTimer.Stop(); // Stop in case it's already running
            _tapTimer.Start();
        } else if (_tapCount == 2) {
            Console.WriteLine("Double Tap Detected.");

            // If a second tap is detected within the timer interval, consider it a double tap
            _tapTimer.Stop(); // Stop the timer since we detected a double tap
            HandleTrackPieceTapped(_selectedTrack, 2);
            _tapCount = 0; // Reset tap count
        }
    }

    private void OnTapTimerElapsed(object? sender, ElapsedEventArgs e) {
        // When the timer elapses, it's a single tap
        if (_tapCount == 1 && _selectedTrack is not null) {
            Console.WriteLine("Single Tap Detected.");
            HandleTrackPieceTapped(_selectedTrack, 1);
        }
        _tapCount = 0; // Reset tap count after handling
    }

    private void DragTrackStarting(DragStartingEventArgs args, ITrackPiece track) {
        Console.WriteLine($"Dragging Track: {track.Name}");
        args.Data.Properties.Add("Track", track);
        args.Data.Properties.Add("Source", "Panel");
    }

    private void DropTrackOnPanel(object? sender, DropEventArgs e) {
        Console.WriteLine("Drop Gesture Recognizer OnDrop");
        try {
            var source = e?.Data?.Properties["Source"] as string ?? null;
            var track  = e?.Data?.Properties["Track"] as ITrackPiece ?? null;
            if (source is null || track is null) {
                Console.WriteLine("Could not determine the source of the item being dropped.");
                return;
            }

            var gridPosition = GetGridPosition(e?.GetPosition(DynamicGrid));
            if (gridPosition is { IsSuccess: true, Value: var position } && track is { } trackPiece) {
                // Make sure that the item we are placing is onto a point that is 
                // not already occupied unless the item being dropped is an overlay 
                // item that has a higher Z factor. 
                // -----------------------------------------------------------------
                if (trackPiece.Layer > GetHighestOccupiedLayer(position.Col, position.Row)) {
                    switch (source) {
                    case "Panel":
                        trackPiece.X = position.Col;
                        trackPiece.Y = position.Row;
                        RebuildGrid(true);
                        break;
                    case "Symbol":
                        var newPiece = Activator.CreateInstance(trackPiece.GetType()) as ITrackPiece;
                        if (newPiece is not null) {
                            newPiece.X = position.Col;
                            newPiece.Y = position.Row;
                            Panel?.Tracks?.Add(newPiece);
                            RebuildGrid(true);
                        } else {
                            Console.WriteLine($"Could not create a new Piece as a TrackPiece.");
                        }

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
        Console.WriteLine($"GetHighestOccupiedLayer({col},{row})");
        var tracksInGrid = Panel?.Tracks.Where(x => x.X == col && x.Y == row) ?? [];
        Console.WriteLine($"GetHighestOccupiedLayer({col},{row}) returned {tracksInGrid?.Count() ?? 0}");
        if (tracksInGrid == null || !tracksInGrid.Any()) return 0;
        return tracksInGrid?.Max(track => track.Layer) ?? 0;
    }
    
    /// <summary>
    /// Convert a position in the grid (absolute) to a Grid position within the col/row definitions
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
    
    private const double Tolerance = 5f;

    public bool HasScreenSizeChanged(double width, double height) {
        return Math.Abs(width - ViewWidth) > Tolerance || Math.Abs(height - ViewHeight) > Tolerance; 
        
        //var gridSize = width > 0 && height > 0 ? Math.Min(width / Cols, height / Rows) / 2 * 2 : 1;
        //var viewWidth = gridSize * Cols;
        //var viewHeight = gridSize * Rows;
        //return Math.Abs(viewWidth - ViewWidth) > Tolerance || Math.Abs(viewHeight - ViewHeight) > Tolerance;
    }

    public void SetScreenSize(double width, double height) {
        GridSize = width > 0 && height > 0 ? Math.Min(width / Cols, height / Rows) / 2 * 2 : 1;
        ViewWidth = GridSize * Cols;
        ViewHeight = GridSize * Rows;
    }

    public void HandleTrackPieceTapped(ITrackPiece track, int taps = 1) {
        if (DesignMode) {
            Console.WriteLine($"In design Mode: Handling {taps} for {track.Name}");
            TrackTapped?.Invoke(this, new TrackSelectedEvent { Track = track, Taps = taps });
        } else {
            if (track is ITrackInteractive) {
                switch (track) {
                case ITrackButton button:
                    Console.WriteLine($"You just tapped on {track.Name} - its a button so we will toggle it. ");
                    button.Clicked();
                    track.NextState();
                    break;
                case ITrackThreeway threeway:
                    Console.WriteLine($"You just tapped on {track.Name} - its a threeway so we will cycle states. ");
                    threeway.Clicked();
                    track.NextState();
                    break;
                case ITrackTurnout turnout:
                    Console.WriteLine($"You just tapped on {track.Name} - its turnout so we will cycle states. ");
                    turnout.Clicked();
                    track.NextState();
                    break;
                }
            }
        }
    }
}

/// <summary>
/// This is a helper class that draws the Grid Lines on the Page.
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