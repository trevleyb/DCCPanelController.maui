using System.ComponentModel;
using DCCPanelController.Tracks.Base;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View {
    public partial class ControlPanelView {

        private ControlPanelViewModel? _viewModel;

        public ControlPanelView() {
            InitializeComponent();
            MainGrid.SizeChanged += OnGridSizeChanged;
            PropertyChanged += OnPropertyChanged;
        }

        protected override void OnBindingContextChanged() {
            if (_viewModel is not null) _viewModel.PropertyChanged -= OnPropertyChanged;
            _viewModel = BindingContext as ControlPanelViewModel;
            if (_viewModel is not null) _viewModel.PropertyChanged += OnPropertyChanged;
            RebuildGrid();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
            case nameof(ControlPanelViewModel.ShowGrid):
                RebuildGrid();
                break;
            }
        }

        private void OnGridSizeChanged(object? sender, EventArgs e) {
            RebuildGrid();
        }

        private void RebuildGrid() {
            if (MainGrid.Width < 1 || MainGrid.Height < 1 || _viewModel is null) return;

            _viewModel.SetScreenSize(MainGrid.Width, MainGrid.Height);

            DynamicGrid.Children.Clear();
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.ColumnDefinitions.Clear();

            // Clear the AbsoluteLayout before adding a new grid and GraphicsView
            if (ControlPanelLayout.Children.Count >= 1) {
                var graphicsViewToRemove = ControlPanelLayout.Children.OfType<GraphicsView>().ToList();
                foreach (var view in graphicsViewToRemove) {
                    ControlPanelLayout.Children.Remove(view);
                }
            }

            for (var i = 0; i < _viewModel.Rows; i++) {
                DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            }

            for (var j = 0; j < _viewModel.Cols; j++) {
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            AddOutlineToGrid();
            AddTrackPiecesToGrid();

            DynamicGrid.WidthRequest = _viewModel.ViewWidth;
            DynamicGrid.HeightRequest = _viewModel.ViewHeight;
        }

        /// <summary>
        /// Draw the Grid Outline
        /// </summary>
        private void AddOutlineToGrid() {
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

        /// <summary>
        /// Add the tracks from the view model onto the Grid
        /// </summary>
        private void AddTrackPiecesToGrid() {
            if (_viewModel is { Panel: { Tracks: { } tracks } }) {
                foreach (var track in tracks) {
                    if (DynamicGrid.ColumnDefinitions.Count >= _viewModel.Cols && DynamicGrid.RowDefinitions.Count >= _viewModel.Rows) {
                        var image = new Image {
                            Scale = 1.5,
                            ZIndex = 5,
                            Rotation = 0,
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
                        
                        // Add the Track Image to the appropriate grid position
                        // ------------------------------------------------------
                        DynamicGrid.SetRow(image, track.Y);
                        DynamicGrid.SetColumn(image, track.X);
                        DynamicGrid.Children.Add(image);
                    }
                }
            }
        }
        
        private void OnTrackPieceTapped(ITrackPiece track) {
            var viewModel = BindingContext as ControlPanelViewModel;
            viewModel?.HandleTrackPieceTapped(track);
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