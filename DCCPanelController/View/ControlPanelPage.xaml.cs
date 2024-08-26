using System.Collections.ObjectModel;
using System.ComponentModel;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;
using DCCPanelController.ViewModel;
using StackExchange.Profiling;

namespace DCCPanelController.View;

public partial class ControlPanelPage : ContentPage, INotifyPropertyChanged {

    private int _lastCols = -1;
    private int _lastRows = -1;
    private Dictionary<(int x, int y), GridPosition> _gridPositions = [];
    private ControlPanelViewModel? viewModel = null;
    private MiniProfiler? profiler;
    
    public ControlPanelPage() {
        profiler = MiniProfiler.StartNew("Testing Grid");
        
        MiniProfiler.Configure(MiniProfiler.DefaultOptions);
        using (profiler.Step("ControlPanelPage")) {
            InitializeComponent();
            BindingContext = viewModel ??= new ControlPanelViewModel(); // todo: Inject
            SizeChanged += OnSizeChanged;
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e) {
        if (sender is ControlPanelPage { Width: > 0, Height: > 0 } page) {
            using (profiler.Step("BuildPanelGrid")) {
                BuildPanelGrid(page.Width, page.Height);
            }

            using (profiler.Step("UpdatePanelGrid")) {
                UpdatePanelGrid();
            }

            Console.WriteLine(profiler.RenderPlainText());
            
        }
    }

    /// <summary>
    /// Build up the Control panel using parameters defined in the Panel Object. This includes adding a Frame to
    /// each grid element so we can control borders etc within that frame.  
    /// </summary>
    private void BuildPanelGrid(double width, double height) {

        if (viewModel == null) return;
        
        viewModel.SetScreenSize(width, height);
        ControlPanelGrid.WidthRequest = viewModel.ViewWidth;
        ControlPanelGrid.HeightRequest = viewModel.ViewHeight;

        if (viewModel.Panel.Cols != _lastCols || viewModel.Panel.Rows != _lastRows) {
            ControlPanelGrid.Children.Clear();
            ControlPanelGrid.RowDefinitions.Clear();
            ControlPanelGrid.ColumnDefinitions.Clear();

            var gridSize = GridLength.Star;
            for (var i = 0; i < viewModel.Panel.Cols; i++) {
                ControlPanelGrid.ColumnDefinitions.Add(new ColumnDefinition(gridSize));
            }

            for (var i = 0; i < viewModel.Panel.Rows; i++) {
                ControlPanelGrid.RowDefinitions.Add(new RowDefinition(gridSize));
            }

            // Add a Frame to each Element. This will allow us to turn the borders of the frame on/off,
            // and we will attach the TrackPiece into the Frame itself.
            // ------------------------------------------------------------------------------------------------
            using (profiler.Step("Adding Grids and Frames")) {
                ControlPanelGrid.BatchBegin(); 
                for (var row = 0; row <= viewModel.Panel.Rows; row++) {
                    for (var col = 0; col <= viewModel.Panel.Cols; col++) {
                        if (!_gridPositions.ContainsKey((col, row))) {
                            var frame = new GridPosition(viewModel, col, row);
                            ControlPanelGrid.Children.Add(frame.Frame);
                            ControlPanelGrid.SetRow(frame.Frame, row);
                            ControlPanelGrid.SetColumn(frame.Frame, col);
                            _gridPositions.Add((col, row), frame);

                        }
                    }
                }
                ControlPanelGrid.BatchCommit();
            }
        }

        _lastCols = viewModel.Panel.Cols;
        _lastRows = viewModel.Panel.Rows;
    }

    /// <summary>
    /// Update or add Track Pieces into the Grid by attaching the track image etc to the Frame element
    /// within the grid. 
    /// </summary>
    public void UpdatePanelGrid() {
        foreach (var gridPosition in _gridPositions.Values) gridPosition.IsUsed = false;
        if (viewModel?.TrackPieces is { } pieces) {
            foreach (var item in pieces) {
                var gridPosition = _gridPositions.ContainsKey((item.X, item.Y)) ? _gridPositions[(item.X, item.Y)] : null;
                gridPosition?.Add(item);
            }
        }
        foreach (var gridPosition in _gridPositions.Values.Where(gridPosition => gridPosition.IsUsed == false)) {
            gridPosition.Clear();
        }
    }

    private class GridPosition {

        private int _x;
        private int _y;
        private bool _isSelected = false;
        private bool _isOccupied = false;
        private readonly ControlPanelViewModel _viewModel;
        
        /// <summary>
        /// Used so that we know if we have updated this cell. If we have not, then we might need to clear
        /// any children. 
        /// </summary>
        public bool IsUsed { get; set; } = false;

        public bool IsSelected {
            get => _isSelected;
            set { _isSelected = value; SetFrameColor();}
        }

        public bool IsOccupied {
            get => _isOccupied;
            set { _isOccupied = value; SetFrameColor();}
        }

        public Frame? Frame { get; init; }
        public Grid? Grid { get; init; }

        private void SetFrameColor() {
            if (Frame != null) Frame.BorderColor = _viewModel.ShowGrid ? Colors.Black : Colors.Transparent;
            if (IsSelected && Frame != null) Frame.BorderColor = Colors.Green;
            if (IsOccupied && Frame != null) Frame.BorderColor = Colors.Red;
        }

        public GridPosition(ControlPanelViewModel viewModel, int x, int y) {
            _viewModel = viewModel;
            _x = x;
            _y = y;

                // Create a Grid that will side inside the Frame so we can add children to the Grid
                // --------------------------------------------------------------------------------
                Grid = new Grid {
                    BackgroundColor = Colors.Transparent,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                };

                Grid.SetBinding(WidthRequestProperty, new Binding(nameof(_viewModel.GridSize)) { Source = _viewModel });
                Grid.SetBinding(HeightRequestProperty, new Binding(nameof(_viewModel.GridSize)) { Source = _viewModel });

            // Create a frame so that we can show a border for a particular cell. This frame will support
            // a grid to hold any child images. 
            // ------------------------------------------------------------------------------------------
            
            Frame = new Frame {
                    Content = Grid,
                    BackgroundColor = Colors.Transparent,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    HasShadow = false
                };

                Frame.SetBinding(Frame.WidthRequestProperty, new Binding(nameof(_viewModel.GridSize)) { Source = _viewModel });
                Frame.SetBinding(Frame.HeightRequestProperty, new Binding(nameof(_viewModel.GridSize)) { Source = _viewModel });
                Frame.SetBinding(Frame.BorderColorProperty, new Binding(nameof(_viewModel.GridColor)) { Source = _viewModel });
        }
        
        public void Clear() => Grid?.Clear();
        public void Add(ITrackPiece track) {
            var image = new Image();
            image.Scale = 1.5;
            image.SetBinding(Image.SourceProperty, new Binding(nameof(track.Image)) { Source = track });
            image.SetBinding(Image.RotationProperty, new Binding(nameof(track.ImageRotation)) { Source = track });
            Grid?.Children.Add(image);
            IsUsed = true;
        }
    }
}
