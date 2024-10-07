using System.ComponentModel;
using System.Net.Quic;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Events;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Helpers;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Layouts;
using StackExchange.Profiling;
using Svg;

//
// This is a COMPONENT that is used inside the operate and panels views
//
namespace DCCPanelController.View {
    public partial class ControlPanelView {

        public event EventHandler<ITrackPiece> TrackPieceTapped; 
        private ControlPanelViewModel? _viewModel;

        public ControlPanelView() {
            InitializeComponent();
            PropertyChanged += OnPropertyChanged;
            MainGrid.SizeChanged += OnGridSizeChanged;
        }

        public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelViewModel), null, BindingMode.TwoWay, propertyChanged: OnPanelChanged);

        public Panel Panel {
            get => (Panel)GetValue(PanelProperty);
            set => SetValue(PanelProperty, value);
        }

        public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelViewModel), null, BindingMode.TwoWay, propertyChanged: OnPanelChanged);

        public bool DesignMode {
            get => (bool)GetValue(DesignModeProperty);
            set => SetValue(DesignModeProperty, value);
        }

        public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelViewModel), null, BindingMode.TwoWay, propertyChanged: OnPanelChanged);

        public bool ShowGrid {
            get => (bool)GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }

        private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
            if (oldValue is Panel { } oldPanel && newValue is Panel { } newPanel) {
                OnPanelChanged(oldPanel, newPanel);
            }
        }

        private static void OnPanelChanged(Panel oldPanel, Panel newPanel) { }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
            case nameof(Panel):
                BindingContext = _viewModel ??= new ControlPanelViewModel(Panel);
                _viewModel.Panel = Panel;
                _viewModel.DesignMode = DesignMode;
                _viewModel.ShowGrid = ShowGrid;
                _viewModel.TrackSelected += (sender, track) => TrackPieceTapped?.Invoke(this, track);
                break;
            case nameof(DesignMode) or nameof(ShowGrid):
                if (_viewModel != null) {
                    _viewModel.DesignMode = DesignMode;
                    _viewModel.ShowGrid = ShowGrid;
                }

                break;
            }
        }

        private void OnGridSizeChanged(object? sender, EventArgs e) {
            RebuildGrid();
        }

        private void RebuildGrid(bool forceRefresh = false) {
            if (_viewModel is null || MainGrid.Width < 1 || MainGrid.Height < 1) return;
            if (!forceRefresh && !_viewModel.HasScreenSizeChanged(MainGrid.Width, MainGrid.Height) ) return;

            using (MiniProfiler.Current.Step("Rebuild Grid")) {

                _viewModel.SetScreenSize(MainGrid.Width, MainGrid.Height);
                DynamicGrid.WidthRequest = _viewModel.ViewWidth;
                DynamicGrid.HeightRequest = _viewModel.ViewHeight;

                DynamicGrid.Children.Clear();
                if (DynamicGrid.RowDefinitions.Count != _viewModel.Rows || DynamicGrid.ColumnDefinitions.Count != _viewModel.Cols) {
                    DynamicGrid.RowDefinitions.Clear();
                    DynamicGrid.ColumnDefinitions.Clear();

                    for (var i = 0; i < _viewModel.Rows; i++) {
                        DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                    }

                    for (var j = 0; j < _viewModel.Cols; j++) {
                        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                    }
                }

                if (ShowGrid) AddOutlineToGrid();
                AddTrackPiecesToGrid();
            }
        }

        public void HighlightCell(int col, int row) {
            if (_viewModel == null) return;
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
            if (_viewModel == null) return;
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
        private void AddOutlineToGrid() {

            using (MiniProfiler.Current.Step("AddOutlineToGrid")) {
                // Clear the AbsoluteLayout before adding a new grid and GraphicsView
                if (ControlPanelLayout.Children.Count >= 1) {
                    var graphicsViewToRemove = ControlPanelLayout.Children.OfType<GraphicsView>().ToList();
                    foreach (var view in graphicsViewToRemove) {
                        ControlPanelLayout.Children.Remove(view);
                    }
                }

                if (_viewModel?.ShowGrid ?? false) {
                    var gridLines = new GridLinesDrawable(_viewModel.Rows, _viewModel.Cols);
                    var graphicsView = new GraphicsView {
                        InputTransparent = true,
                        Drawable = gridLines,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };

                    // Add the GraphicsView directly to the AbsoluteLayout
                    AbsoluteLayout.SetLayoutBounds(graphicsView, new Rect(0.5, 0.5, _viewModel.ViewWidth, _viewModel.ViewHeight));
                    AbsoluteLayout.SetLayoutFlags(graphicsView, AbsoluteLayoutFlags.PositionProportional);
                    ControlPanelLayout.Children.Add(graphicsView);
                    graphicsView.Invalidate();
                }
            }
        }

        /// <summary>
        /// Add the tracks from the view model onto the Grid
        /// </summary>
        private void AddTrackPiecesToGrid() {
            using (MiniProfiler.Current.Step("AddTrackPiecesToGrid")) {
                if (_viewModel is { Panel: { Tracks: { } tracks } panel }) {
                    foreach (var track in tracks) {
                        if (DynamicGrid.ColumnDefinitions.Count >= _viewModel.Cols && DynamicGrid.RowDefinitions.Count >= _viewModel.Rows && track.X < _viewModel.Cols && track.Y < _viewModel.Rows) {
                            var image = AddImageToLayout(track);
                        }

                        // If we need to overlay Valid/Invalid Options. Work out the points and draw error boxes
                        // -------------------------------------------------------------------------------------
                        if (_viewModel.ShowTrackErrors) {
                            var pointImage = new TrackPoints { X = track.X, Y = track.Y };
                            var validPoints = TrackPointsValidator.GetConnectedTracksStatus(tracks, track, panel.Cols, panel.Rows);
                            pointImage.SetPoints(validPoints);
                            var image = AddImageToLayout(pointImage);
                            image.InputTransparent = true;
                        }
                    }
                }
            }
        }

        private void RemoveImageFromLayout(ITrackPiece track) {
            var tracksInGrid = DynamicGrid.Children;
        }

        private Image AddImageToLayout(ITrackPiece track) {
            using (MiniProfiler.Current.Step("AddImageToGridLayout")) {
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
                image.SetBinding(Image.RotationProperty, new Binding(nameof(track.ImageRotation)) { Source = track });
                image.SetBinding(WidthRequestProperty, new Binding(nameof(_viewModel.GridSize)) { Source = _viewModel });
                image.SetBinding(HeightRequestProperty, new Binding(nameof(_viewModel.GridSize)) { Source = _viewModel });

                // Setup trigger control to trap if we click on or select the track item
                // -------------------------------------------------------------------------------------------
                // Create TapGestureRecognizer
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnTrackPieceTapped(track);
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
        }

        private void OnTrackPieceTapped(ITrackPiece track) {
            _viewModel?.HandleTrackPieceTapped(track);
        }

        private void DropGestureRecognizer_OnDrop(object? sender, DropEventArgs e) {
            try {
                e.Data.Properties.TryGetValue("Source", out var source);
                e.Data.Properties.TryGetValue("Track", out var track);

                var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));
                if (gridPosition is { IsSuccess: true, Value: var position } && track is ITrackPiece trackPiece) {

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
                                _viewModel?.Panel?.Tracks?.Add(newPiece);
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
            var tracksInGrid = _viewModel?.Panel?.Tracks.Where(x => x.X == col && x.Y == row);
            Console.WriteLine($"GetHighestOccupiedLayer({col},{row}) returned {tracksInGrid?.Count() ?? 0}");
            if (tracksInGrid == null || !tracksInGrid.Any()) return 0;
            return tracksInGrid?.Max(track => track.Layer) ?? 0;
        }
        
        private void DragTrackStarting(DragStartingEventArgs args, ITrackPiece track) {
            Console.WriteLine($"Dragging Track: {track.Name}");
            args.Data.Properties.Add("Track", track);
            args.Data.Properties.Add("Source", "Panel");
        }

        
        
        //private void TapGestureRecognizer_OnTapped(object? sender, TappedEventArgs e) {
        //    Console.WriteLine($"Check the buttons: Mask = {e.Buttons}");
        //    if (sender is Grid grid) {
        //        var position = e.GetPosition(grid);
        //        var gridPosition = GetGridPosition(position);
        //        Console.WriteLine($"Tapped at {position?.X}, {position?.Y} ==> {gridPosition?.Col},{gridPosition?.Row}");
        //    }
        //}

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
            return Result<(int Col, int Row)>.Failure("Could not determine the Grid Position from the point provided,");
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
                canvas.StrokeSize = (i == 0 || i == rows) ? GridWidth : LineWidth;
                canvas.DrawLine(0, i * cellHeight, dirtyRect.Width, i * cellHeight);
            }

            for (var j = 0; j <= columns; j++) {
                canvas.StrokeSize = (j == 0 || j == columns) ? GridWidth : LineWidth;
                canvas.DrawLine(j * cellWidth, 0, j * cellWidth, dirtyRect.Height);
            }
        }
    }
}