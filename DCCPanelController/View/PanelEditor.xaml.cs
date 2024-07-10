using System.Collections.Specialized;
using DCCPanelController.Symbols.Tracks;
using DCCPanelController.Symbols.TrackViewModels;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {

    private readonly PanelEditorViewModel _viewModel;
    private Border? _dropZoneView;
    private int _lastX;
    private int _lastY;

    public PanelEditor() {
        InitializeComponent();
        _viewModel = new PanelEditorViewModel();
        _viewModel.Tracks.CollectionChanged += TracksOnCollectionChanged;
        PanelEditorViewPane.SizeChanged += PanelEditorContainerSizeChanged;
        BindingContext = _viewModel;
    }

    private void PanelEditorContainerSizeChanged(object? sender, EventArgs e) {
        if (sender is AbsoluteLayout layout) {
            // Set the Panel View Bounds in the View Model
            _viewModel.SetPanelEditorBounds((int)layout.Width, (int)layout.Height);
        }
    }

    /// <summary>
    /// Manual Create/Update the display of the Tracks on the Screen as using a CollectionView
    /// was not working. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void TracksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        
        // Add any new items added to the collection.
        // ---------------------------------------------------------
        if (e.NewItems is not null && e.NewItems.Count > 0) {
            foreach (var item in e.NewItems) {
                if (item is ITrackViewModel viewModel) {
                    var track = new TrackView(viewModel);
                    AbsoluteLayout.SetLayoutBounds(track, viewModel.Bounds);
                    AbsoluteLayout.SetLayoutFlags(track, AbsoluteLayoutFlags.None);
                    
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += TapGestureRecognizerOnTapped;
                    track.GestureRecognizers.Add(tapGestureRecognizer);
                    
                    var dragGestureRecognizer = new DragGestureRecognizer {
                        DragStartingCommand = ((PanelEditorViewModel)this.BindingContext).TrackDragCommand,
                        DragStartingCommandParameter = track
                    };

                    track.GestureRecognizers.Add(dragGestureRecognizer);
                    
                    PanelEditorViewPane.Children.Add(track);
                }
            }
        }
        
        // Remove any deleted items added to the collection.
        // ---------------------------------------------------------
        if (e.OldItems is not null && e.OldItems.Count > 0) {
            foreach (var item in e.OldItems) {
                if (item is ITrackViewModel track) {
                    var itemsToDelete = PanelEditorViewPane.Children.OfType<TrackView>().Where(view => view.ViewModel.Track.Coordinate.Equals(track.Track.Coordinate)).ToList();

                    foreach (var view in itemsToDelete) {
                        PanelEditorViewPane.Children.Remove(view);
                    }
                }
            }
        }
    }

    // Toggle the Selected Track so that we can take actions against it
    // ------------------------------------------------------------------------
    private void TapGestureRecognizerOnTapped(object? sender, TappedEventArgs e) {
        switch (sender) {
        case TrackView trackView:
            _viewModel.SetSelectedTrack(trackView.ViewModel);
            break;
        case AbsoluteLayout layout when layout == PanelEditorViewPane:
            _viewModel.SetSelectedTrack(null);
            break;
        }
    }

    private void TapGestureRecognizerBlankEditor(object? sender, TappedEventArgs e) {
        if (sender is AbsoluteLayout layout && layout == PanelEditorViewPane) {
            _viewModel.SetSelectedTrack(null);
        }
    }

    private void DropGestureRecognizer_OnDragOver(object? sender, DragEventArgs e) {
        var pos = e.GetPosition(PanelEditorViewPane);
        if (pos.HasValue) {
            // This stores the last Coordinates and returns a new X,Y which is the centre of the grid.
            // ---------------------------------------------------------------------------------------
            var loc = _viewModel.SetLastCoordinates((int)pos.Value.X, (int)pos.Value.Y);
            if (loc is { XOffset: > -1, YOffset: > -1 }) {
                DrawDropZone(loc.XOffset, loc.YOffset);
                return;
            }
        }
        RemoveDropZone();
    }

    private void DropGestureRecognizer_OnDragLeave(object? sender, DragEventArgs e) {
        if (_dropZoneView is not null) {
            RemoveDropZone();
        }
    }

    private void DropGestureRecognizer_OnDrop(object? sender, DropEventArgs e) {
        if (_dropZoneView is not null) {
            RemoveDropZone();
        }
    }

    #region Manage the Drop Zone Box that is used for Drag/Drop Operations
    
    private void RemoveDropZone() {
        PanelEditorViewPane.Children.Remove(_dropZoneView);
        _dropZoneView = null;
    }

    private void DrawDropZone(int x, int y) {
        var loc = _viewModel.SetLastCoordinates(x, y);
        if (loc is { XOffset: > -1, YOffset: > -1 }) {
            if (loc.XCenter != _lastX || loc.YCenter != _lastY) {
                _lastX = loc.XOffset;
                _lastY = loc.YOffset;
                SetDropZoneView(_lastX, _lastY, loc.BoxSize, loc.BoxSize);
            }
        }
    }
    
    private void SetDropZoneView(int x, int y, int width, int height) {
        if (_dropZoneView is null) {
            _dropZoneView = new Border( );
            if (Application.Current != null && Application.Current.Resources.TryGetValue("CardView", out var cardViewStyle)) _dropZoneView.Style = (Style) cardViewStyle;
            PanelEditorViewPane.Children.Add(_dropZoneView);
        }

        var xWidth   = width + 10;   // Adding some bounds AROUND the drag object
        var yHeight  = height + 10; // Adding some bounds AROUND the drag object
        var xPos = x - (xWidth / 2);
        var yPos = y - (yHeight / 2);
        
        PanelEditorViewPane.SetLayoutBounds(_dropZoneView, new Rect(xPos, yPos, xWidth, yHeight)); // X=50, Y=100, Width=200, Height=200
        PanelEditorViewPane.SetLayoutFlags(_dropZoneView, AbsoluteLayoutFlags.None);               // None means the Rectangle properties are absolute values
    }
    #endregion
}