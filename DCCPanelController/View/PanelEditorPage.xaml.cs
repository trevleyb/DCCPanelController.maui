using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components;
using DCCPanelController.Components.DropZone;
using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Elements.Views;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {

    private readonly ObservableCollection<Line> _lines = [];
    private readonly PanelEditorViewModel _viewModel;

    private DropZone? _dropZone;
    private int _lastX;
    private int _lastY;
    
    public event Action<Panel>? OnFinished;
    protected override void OnDisappearing() {
        base.OnDisappearing();
        OnFinished?.Invoke(_viewModel.Panel);
    }

    public PanelEditorPage(Panel panel) {
        InitializeComponent();
        _viewModel = new PanelEditorViewModel(panel);
        _viewModel.PanelElements.CollectionChanged += PanelElementsOnCollectionChanged;
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
            DrawBorder();
            DrawGridLines();    // Need to do this before we draw the tracks
        }
    }

    private void DrawBorder() { }

    private void DrawGridLines() {
        // Remove all of them ensuring each is evented so the UI updates
        // Have to copy the collection first so we don't modify a collection we are iterating on
        if (_lines.Any()) {
            var removeLines = _lines.ToList();
            foreach (var line in removeLines) {
                PanelEditorViewPane.Children.Remove(line);
                _lines.Remove(line);
            }
        }

        if (_viewModel.GridHelper is not null) {
            var gridSize = _viewModel.GridHelper.BoxSize;
            var height = _viewModel.GridHelper.BoxSize * _viewModel.GridHelper.PanelRows;
            var width = _viewModel.GridHelper.BoxSize * _viewModel.GridHelper.PanelCols;
            
            for (var i = 1; i < _viewModel.Panel.Cols; i++) {
                AddGridLine(i * gridSize, i * gridSize, 0, height, (i==0 || i==_viewModel.Panel.Cols));
            }
            for (var i = 1; i < _viewModel.Panel.Rows; i++) {
                AddGridLine(0, width, i * gridSize, i * gridSize, (i==0 || i==_viewModel.Panel.Rows));
            }
        }
    }

    private void AddGridLine(int x1, int x2, int y1, int y2, bool isEdge) {
        var line = new Line() { X1 = x1, X2 = x2, Y1 = y1, Y2 = y2,
            IsEnabled = false, ZIndex = 5, Stroke = Colors.DarkGray, StrokeThickness = 1,
        };
        _lines.Add(line);
        PanelEditorViewPane.Children.Add(line);
    }
    
    /// <summary>
    /// Manual Create/Update the display of the Tracks on the Screen as using a CollectionView
    /// was not working. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void PanelElementsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {

        switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
            // Add any new items added to the collection.
            // ---------------------------------------------------------
            if (e.NewItems is not null && e.NewItems.Count > 0) {
                foreach (var item in e.NewItems) {
                    if (item is IElementView elementView) {
                        if (elementView is Microsoft.Maui.Controls.View view) {
                            AbsoluteLayout.SetLayoutBounds(view, elementView.ViewModel.Bounds);
                            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);

                            var tapGestureRecognizer = new TapGestureRecognizer();
                            tapGestureRecognizer.Tapped += TapGestureRecognizerOnTapped;
                            view.GestureRecognizers.Add(tapGestureRecognizer);

                            var dragGestureRecognizer = new DragGestureRecognizer {
                                DragStartingCommand = ((PanelEditorViewModel)this.BindingContext).ElementDragCommand,
                                DragStartingCommandParameter = view
                            };

                            view.ZIndex = 10;
                            view.GestureRecognizers.Add(dragGestureRecognizer);
                            PanelEditorViewPane.Children.Add(view);
                        }
                    }
                }
            }
            break;
        
        case NotifyCollectionChangedAction.Remove:
            // Remove any deleted items added to the collection.
            // ---------------------------------------------------------
            if (e.OldItems is not null && e.OldItems.Count > 0) {
                foreach (var item in e.OldItems) {
                    if (item is IElementView elementView) {
                        var itemsToDelete = PanelEditorViewPane.Children.OfType<IElementView>().Where(view => view.ViewModel.Element.Coordinate.Equals(elementView.ViewModel.Element.Coordinate)).ToList();
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
            _viewModel.SetSelectedTrack(trackView);
            break;
        case AbsoluteLayout layout when layout == PanelEditorViewPane:
            _viewModel.SetSelectedTrack(null);
            break;
        }
    }

    private void TapGestureRecognizerBlankEditor(object? sender, TappedEventArgs e) {
        _viewModel.SetSelectedTrack(null);
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
        if (coordinate.Col != _lastX || coordinate.Row != _lastY) {
            DrawDropZone(coordinate);
        }
    }
    
    private void DrawDropZone(Coordinate coordinate) {
        
        var loc = _viewModel.GridHelper?.GetGridCoordinates(coordinate);
        if (loc is null) return;
        
        if (_dropZone is null) {
            _dropZone = new DropZone(_viewModel) { ZIndex = 20 };
            PanelEditorViewPane.SetLayoutBounds(_dropZone, new Rect(loc.XOffset, loc.YOffset, loc.BoxSize, loc.BoxSize)); // X=50, Y=100, Width=200, Height=200
            PanelEditorViewPane.SetLayoutFlags(_dropZone, AbsoluteLayoutFlags.None);          // None means the Rectangle properties are absolute values
            PanelEditorViewPane.Children.Add(_dropZone);
        } else {
            PanelEditorViewPane.SetLayoutBounds(_dropZone, new Rect(loc.XOffset, loc.YOffset, loc.BoxSize, loc.BoxSize)); // X=50, Y=100, Width=200, Height=200
            PanelEditorViewPane.SetLayoutFlags(_dropZone, AbsoluteLayoutFlags.None);          // None means the Rectangle properties are absolute values
        }
        _lastX = coordinate.Col;
        _lastY = coordinate.Row;
    }
    #endregion

    #region Manage the buttons on the toolbar if needed
    private void MultiSelectToolbarItem_OnClicked(object? sender, EventArgs e) {
        MultiSelectToolbarItem.IconImageSource = _viewModel.MultiSelectMode ? "deselect.png" : "select.png";
    }

    private void PropertiesToolbarItem_OnClicked(object? sender, EventArgs e) {
        if (!_viewModel.IsPropertyAllowed) {
            if (_viewModel.IsPropertyPanelVisible) {
                _viewModel.IsPropertyPanelVisible = false;
            } else {
                _viewModel.IsPropertyPanelVisible = true;
            }
        }
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