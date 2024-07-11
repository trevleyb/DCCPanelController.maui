using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Symbols;
using DCCPanelController.Symbols.Tracks;
using DCCPanelController.Symbols.TrackViewModels;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {

    private readonly PanelEditorViewModel _viewModel;
    private DropZone? _dropZone;
    private int _lastX;
    private int _lastY;

    public PanelEditor() {
        InitializeComponent();
        _viewModel = new PanelEditorViewModel();
        _viewModel.Tracks.CollectionChanged += TracksOnCollectionChanged;
        PanelEditorContainer.SizeChanged += PanelEditorContainerSizeChanged;
        BindingContext = _viewModel;
    }
    
    private void PanelEditorContainerSizeChanged(object? sender, EventArgs e) {
        if (sender is AbsoluteLayout layout && sender == PanelEditorContainer) {
            _viewModel.SetPanelEditorBounds((int)layout.Width, (int)layout.Height);
            ResizePanelViewArea();
        }
    }

    private void ResizePanelViewArea() {
        if (_viewModel is { GridHelper: { } grid }) {
            var rect = new Rect(grid.XMargin, grid.YMargin, grid.PanelWidth, grid.PanelHeight);
            PanelEditorContainer.SetLayoutBounds(PanelEditorViewPane, rect);
            PanelEditorContainer.SetLayoutFlags(PanelEditorViewPane,AbsoluteLayoutFlags.None);
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

        switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
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
            break;
        case NotifyCollectionChangedAction.Remove:
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
            break;
        case NotifyCollectionChangedAction.Reset:
            var itemsToReset = PanelEditorViewPane.Children.OfType<TrackView>().ToList();
            foreach (var view in itemsToReset) {
                PanelEditorViewPane.Children.Remove(view);
            }
            break;
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
    
    private void Symbol_OnDragStarting(object? sender, DragStartingEventArgs e) {
        //if (sender is DragGestureRecognizer { Parent: SymbolView symbolView }) {
        //    symbolView.Scale = 0.5;
        //}
    }
    
    private void Symbol_OnDragFinished(object? sender, DropCompletedEventArgs e) {
        //if (sender is DragGestureRecognizer { Parent: SymbolView symbolView }) {
        //    symbolView.Scale = 1.0;
        //}
    }


    private void DropGestureRecognizer_OnDragOver(object? sender, DragEventArgs e) {
        var pos = e.GetPosition(PanelEditorViewPane);
        if (pos.HasValue) {
            // This stores the last Coordinates and returns a new X,Y which is the centre of the grid.
            // ---------------------------------------------------------------------------------------
            var coordinate = _viewModel.SetLastCoordinates((int)pos.Value.X, (int)pos.Value.Y);
            CheckDropZone(coordinate);
        }
    }

    private void DropGestureRecognizer_OnDragLeave(object? sender, DragEventArgs e) {
        var pos = e.GetPosition(PanelEditorViewPane);
        if (pos.HasValue && _viewModel.GridHelper != null && 
            (pos.Value.X < 0 || pos.Value.Y < 0 || 
             pos.Value.X > _viewModel.GridHelper.ViewWidth || 
             pos.Value.Y > _viewModel.GridHelper.ViewHeight) 
            && _dropZone is not null) {
            RemoveDropZone();
        } 
    }

    private void DropGestureRecognizer_OnDrop(object? sender, DropEventArgs e) {
        if (_dropZone is not null) {
            RemoveDropZone();
        }
    }

    #region Manage the Drop Zone Box that is used for Drag/Drop Operations
    
    private void RemoveDropZone() {
        PanelEditorViewPane.Children.Remove(_dropZone);
        _dropZone = null;
    }

    private void CheckDropZone(Coordinate coordinate) {
        if (coordinate.Column != _lastX || coordinate.Row != _lastY) {
            DrawDropZone(coordinate);
        }
    }
    
    private void DrawDropZone(Coordinate coordinate) {
        
        var loc = _viewModel.GridHelper.GetGridCoordinates(coordinate);
        
        if (_dropZone is null) {
            _dropZone = new DropZone(_viewModel);
            PanelEditorViewPane.SetLayoutBounds(_dropZone, new Rect(loc.XOffset, loc.YOffset, loc.BoxSize, loc.BoxSize)); // X=50, Y=100, Width=200, Height=200
            PanelEditorViewPane.SetLayoutFlags(_dropZone, AbsoluteLayoutFlags.None);          // None means the Rectangle properties are absolute values
            PanelEditorViewPane.Children.Add(_dropZone);
        } else {
            PanelEditorViewPane.SetLayoutBounds(_dropZone, new Rect(loc.XOffset, loc.YOffset, loc.BoxSize, loc.BoxSize)); // X=50, Y=100, Width=200, Height=200
            PanelEditorViewPane.SetLayoutFlags(_dropZone, AbsoluteLayoutFlags.None);          // None means the Rectangle properties are absolute values
        }
        _lastX = coordinate.Column;
        _lastY = coordinate.Row;
    }
    #endregion

    #region Manage the buttons on the toolbar if needed
    private void MultiSelectToolbarItem_OnClicked(object? sender, EventArgs e) {
        MultiSelectToolbarItem.IconImageSource = _viewModel.MultiSelectMode ? "deselect.png" : "select.png";
    }

    private void PropertiesToolbarItem_OnClicked(object? sender, EventArgs e) {
    }

    private void DeleteTrackToolbarItem_OnClicked(object? sender, EventArgs e) {
    }

    private void RotateTrackToolbarItem_OnClicked(object? sender, EventArgs e) {
    }
    #endregion

    private void Stepper_OnValueChanged(object? sender, ValueChangedEventArgs e) {
        ResizePanelViewArea();
    }

}