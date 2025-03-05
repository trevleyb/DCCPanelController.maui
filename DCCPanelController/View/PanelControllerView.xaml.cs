using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View;

public partial class PanelControllerView : ContentView {

    private const double DefaultZoomFactor = 1.0;
    private const double ZoomFactorIncrements = 0.25;
    private const double MaxZoomFactor = 3.0;
    private const double MinZoomFactor = 0.50;

    public PanelControllerView() {
        InitializeComponent();
        BindingContext = this;
        MainGrid.SizeChanged += MainGridOnSizeChanged;
        PropertyChanged += OnPropertyChanged;
    }

    private float GridSize { get; set; }

    #region Event Handlers
    public event EventHandler<ITrack>? TrackPieceTapped;
    public event EventHandler<ITrack>? TrackPieceDoubleTapped;
    
    protected void OnTrackPieceTapped(ITrack e) {
        Console.WriteLine($"PanelControllerView.OnTrackPieceTapped: {e.Name}={e.UniqueID}");
        TrackPieceTapped?.Invoke(this, e);
    }

    protected void OnTrackPieceDoubleTapped(ITrack e) {
        Console.WriteLine($"PanelControllerView.OnTrackPieceDoubleTapped: {e.Name}={e.UniqueID}");
        TrackPieceDoubleTapped?.Invoke(this, e);
    }
    #endregion

    #region Bindable Properties
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(PanelControllerView), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(PanelControllerView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(PanelControllerView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty ZoomFactorProperty = BindableProperty.Create(nameof(ZoomFactor), typeof(double), typeof(PanelControllerView), DefaultZoomFactor, BindingMode.Default, propertyChanged: OnZoomFactorChanged);
    public static readonly BindableProperty GridColorProperty = BindableProperty.Create(nameof(GridColor), typeof(Color), typeof(PanelControllerView), Colors.LightGray, BindingMode.Default);
    public static readonly BindableProperty GridLineSizeProperty = BindableProperty.Create(nameof(GridLineSize), typeof(float), typeof(PanelControllerView), 0.5f, BindingMode.Default);

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

    public float GridLineSize {
        get => (float)GetValue(GridLineSizeProperty);
        set => SetValue(GridLineSizeProperty, value);
    }
    
    public double ZoomFactor {
        get => (double)GetValue(ZoomFactorProperty);
        set {
            if (value > MaxZoomFactor) value = MaxZoomFactor;
            if (value < MinZoomFactor) value = MinZoomFactor;
            if (value == 0) value = DefaultZoomFactor;
            SetValue(ZoomFactorProperty, value);
        }
    }

    public bool IsNotBusy => !IsBusy;
    public bool IsBusy {
        get;
        set {
            field = value;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsNotBusy));
        }
    } = false;

    private static void OnShowGridChanged(BindableObject bindable, object oldValue, object newValue) {
        Console.WriteLine($"PanelControllerView.OnShowGridChanged: {oldValue} -> {newValue}");
        var control = (PanelControllerView)bindable;
        control.DrawGridLines();
    }

    private static void OnDesignModeChanged(BindableObject bindable, object oldValue, object newValue) {
        Console.WriteLine($"PanelControllerView.OnDesignModeChanged: {oldValue} -> {newValue}");
        //var control = (PanelControllerView)bindable;
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        Console.WriteLine($"PanelControllerView.OnPanelChanged: {oldValue} -> {newValue}");
        var control = (PanelControllerView)bindable;
        if (oldValue is Panel oldPanel) {
            oldPanel.PropertyChanged -= control.OnPropertyChanged;
        }
        if (newValue is Panel newPanel) {
            newPanel.PropertyChanged += control.OnPropertyChanged; 
        }
    }

    private static void OnZoomFactorChanged(BindableObject bindable, object oldValue, object newValue) {
        Console.WriteLine($"PanelControllerView.OnZoomFactorChanged: {oldValue} -> {newValue}");
        var control = (PanelControllerView)bindable;
        if (Math.Abs((double)oldValue - (double)newValue) >= ZoomFactorIncrements) control.MainGridOnSizeChanged(null, EventArgs.Empty);
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) {
        Console.WriteLine($"PanelControllerView.PropertyChanged: {args.PropertyName} from {sender?.GetType().ToString() ?? "unknown sender"}");

        switch (sender) {
        case ITrack track:
            break;
        case Panel panel:
            UpdateView();
            break;
        default:
            break;
        }
        
    }
    
    #endregion

    #region Grid Size and Drawing
    /// <summary>
    /// When the main view changes its size, we need to work out the dimensions that we can
    /// fit into the main view. We then adjust and control the panel that is show by making
    /// sure that the grid dimensions are the same and will fit.
    ///
    /// As this can be called multiple times, only redraw or adjust IF there has been a
    /// material change in the size of our display.  
    /// </summary>
    private void MainGridOnSizeChanged(object? sender, EventArgs e) {
        Console.WriteLine($"MainGridOnSizeChanged: Container={ContainerGrid.Width},{ContainerGrid.Height} MainGrid={MainGrid.Width},{MainGrid.Height}");
        if (Panel is null) return;
        var gridSize = CalculateGridSize(MainGrid.Width, MainGrid.Height);
        if (gridSize < 1.0f || Math.Abs(gridSize - GridSize) < 1.0f) return;
        GridSize = gridSize;
        ContainerGrid.WidthRequest = Panel.Cols * GridSize;
        ContainerGrid.HeightRequest = Panel.Rows * GridSize;
        UpdateView();
    }

    /// <summary>
    /// When a control is first created, it sends a -1 for width and -1 for height.
    /// We should just ignore any sizes that are less than 1.0
    /// </summary>
    public float CalculateGridSize(double width, double height) {
        if (Panel is null || width <= 0 || height <= 0) return -1;
        return (float)Math.Round(Math.Min(width / Panel.Cols, height / Panel.Rows));
    }

    /// <summary>
    /// This completely redraws the whole view, including the grid lines (if turned on). 
    /// </summary>
    // ReSharper disable once AsyncVoidMethod
    public async void UpdateView() => await UpdateViewAsync();
    public async Task UpdateViewAsync() {
        if (Panel?.Tracks?.Count > 0 && GridSize > 1.0) {
            var stopwatch = Stopwatch.StartNew();
            IsBusy = true;
            MainGrid.Children.Clear();
            await Task.WhenAll(
                DrawGridLinesAsync(),
                AddTracksAsync()
            );
            IsBusy = false;
            stopwatch.Stop();
            Console.WriteLine($"PanelControllerView.UpdateView: {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    private async Task AddTracksAsync() {
        foreach (var track in Panel?.Tracks ?? []) AddTrack(track);
    }

    private void AddTrack(ITrack track) {
        if (CreateTrackView(track) is { } trackView) {
            trackView.ClassId = track.UniqueID.ToString();
            
            // Setup trigger control to trap if we click on or select the track item
            // -------------------------------------------------------------------------------------------
            // Create Single-Tap Gesture
            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tapGesture.Tapped += (_, _) => OnTrackPieceTapped(track);
            trackView.GestureRecognizers.Add(tapGesture);

            // Create Double-Tap Gesture
            var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTapGesture.Tapped += (_, _) => OnTrackPieceDoubleTapped(track);
            trackView.GestureRecognizers.Add(doubleTapGesture);

            // If we are in Design mode, then add support for dragging and dropping
            // ---------------------------------------------------------------------------------------
            if (DesignMode) {
                //var dragGesture = new DragGestureRecognizer();
                //dragGesture.DragStarting += (sender, args) => DragTrackStarting(args, track);
                //    view.GestureRecognizers.Add(dragGesture);
                //}
            }
            track.PropertyChanged += OnPropertyChanged;
            MainGrid.Children.Add(trackView);
        }
    }

    private void RemoveTrack(ITrack track) {
        var tracks = MainGrid.Children.Where(child => (child as Microsoft.Maui.Controls.View)?.ClassId == track.UniqueID.ToString()).ToList();
        foreach (var found in tracks) {
            track.PropertyChanged -= OnPropertyChanged;
            MainGrid.Children.Remove(found);
        }
    }

    /// <summary>
    /// Creates a visual representation of a track using its layout information and grid size.
    /// Adjusts the track view's position and size based on track properties and places it within the grid layout.
    /// </summary>
    /// <param name="track">The track object containing layout and display information.</param>
    /// <returns>A configured view representing the track, or null if the track view could not be created.</returns>
    private Microsoft.Maui.Controls.View? CreateTrackView(ITrack track) {
        if (track.TrackView(GridSize, false) is not Microsoft.Maui.Controls.View { } trackImage) return null;
        AbsoluteLayout.SetLayoutBounds(trackImage, new Rect(track.X * GridSize, track.Y * GridSize, GridSize, GridSize));
        AbsoluteLayout.SetLayoutFlags(trackImage, AbsoluteLayoutFlags.None);
        return trackImage; 
    }

    // ReSharper disable once AsyncVoidMethod
    private async void DrawGridLines() => await DrawGridLinesAsync();
    private async Task DrawGridLinesAsync() {
        await RemoveGridLinesAsync();
        if (!ShowGrid || Panel?.Rows <= 1 || Panel?.Cols <= 1) return;
        var gridLines = new GridLinesDrawable(Panel?.Rows ?? 1, Panel?.Cols ?? 1, GridColor, GridLineSize);
        var graphicsView = new GraphicsView {
            InputTransparent = true,
            Drawable = gridLines,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            ClassId = "GridLine",
        };
        AbsoluteLayout.SetLayoutBounds(graphicsView, new Rect(0, 0, MainGrid.Width, MainGrid.Height));
        AbsoluteLayout.SetLayoutFlags(graphicsView, AbsoluteLayoutFlags.None);
        MainGrid.Children.Add(graphicsView);
    }
    
    /// <summary>
    /// Remove the Grid Lines. Should only be one as it is a drawing object. 
    /// </summary>
    private async Task RemoveGridLinesAsync() {
        var gridLines = MainGrid.Children.Where(child => (child as Microsoft.Maui.Controls.View)?.ClassId == "GridLine").ToList();
        foreach (var gridLine in gridLines) {
            MainGrid.Children.Remove(gridLine);
        }
    }

    #endregion
    
    #region Drag and Drop Events
    #endregion
}

public enum CellHighlightType {
    Selected,
    DragInvalid,
    DragValid
}
