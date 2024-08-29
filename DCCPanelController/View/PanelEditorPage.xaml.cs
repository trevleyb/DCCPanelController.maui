using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DCCPanelController.Components.DropZone;
using DCCPanelController.Components.Elements.Views;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using OnScreenSizeMarkup.Maui.Helpers;
/*
namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {

    private readonly ObservableCollection<Line> _lines = [];
    private readonly PanelEditorViewModel _viewModel;

    private DropZone? _dropZone;
    private int _lastX;
    private int _lastY;

    public event Action<Panel>? OnFinished;

    public PanelEditorPage(Panel panel) {
        InitializeComponent();
        _viewModel = new PanelEditorViewModel(panel);
        _viewModel.PanelElements.CollectionChanged += PanelElementsOnCollectionChanged;
        PanelEditorContainer.SizeChanged += PanelEditorContainerSizeChanged;
        BindingContext = _viewModel;

        DrawColors();

        //RightPanelColumn.Width = new GridLength(200, GridUnitType.Absolute);
        //var panGesture = new PanGestureRecognizer();
        //panGesture.PanUpdated += OnPanUpdated;
        //Toolbox.GestureRecognizers.Add(panGesture);
    }

    void OnPanUpdated(object? sender, PanUpdatedEventArgs e) {
        switch (e.StatusType) {
        case GestureStatus.Running:
            double currentWidth = RightPanelColumn.Width.Value + e.TotalX;
            if (currentWidth > 50) {
                RightPanelColumn.Width = new GridLength(currentWidth, GridUnitType.Absolute);
            }

            break;

        // Handle other statuses as needed
        }
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        OnFinished?.Invoke(_viewModel.Panel);
    }

    private void PanelEditorContainerSizeChanged(object? sender, EventArgs e) {
        if (sender is AbsoluteLayout layout && sender == PanelEditorContainer) {
            //_viewModel.SetPanelEditorBounds((int)layout.Width, (int)layout.Height);
            ResizePanelViewArea();
        }
    }

    private void ResizePanelViewArea() {
        if (_viewModel is { GridHelper: { } grid }) {
            var rect = new Rect(grid.XMargin, grid.YMargin, grid.PanelWidth, grid.PanelHeight);
            PanelEditorContainer.SetLayoutBounds(PanelEditorViewPane, rect);
            PanelEditorContainer.SetLayoutFlags(PanelEditorViewPane, AbsoluteLayoutFlags.None);
            DrawGridLines(); // Need to do this before we draw the tracks
        }
    }

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
                AddGridLine(i * gridSize, i * gridSize, 0, height, (i == 0 || i == _viewModel.Panel.Cols));
            }

            for (var i = 1; i < _viewModel.Panel.Rows; i++) {
                AddGridLine(0, width, i * gridSize, i * gridSize, (i == 0 || i == _viewModel.Panel.Rows));
            }
        }
    }

    private void AddGridLine(int x1, int x2, int y1, int y2, bool isEdge) {
        var line = new Line() {
            X1 = x1, X2 = x2, Y1 = y1, Y2 = y2,
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
            var itemsToReset = PanelEditorViewPane.Children.OfType<IElementView>().ToList();
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
        case IElementView trackView:
            //_viewModel.SetSelectedTrack(trackView);
            break;
        case AbsoluteLayout layout when layout == PanelEditorViewPane:
            //_viewModel.SetSelectedTrack(null);
            break;
        }
    }

    private void TapGestureRecognizerBlankEditor(object? sender, TappedEventArgs e) {
        //_viewModel.SetSelectedTrack(null);
    }

    private void DropGestureRecognizer_OnDragOver(object? sender, DragEventArgs e) {
        var pos = e.GetPosition(PanelEditorViewPane);
        if (pos.HasValue) {
            // This stores the last Coordinates and returns a new X,Y which is the centre of the grid.
            // ---------------------------------------------------------------------------------------
            //var coordinate = _viewModel.SetLastCoordinates((int)pos.Value.X, (int)pos.Value.Y);
            //CheckDropZone(coordinate);
        }
    }

    private void DropGestureRecognizer_OnDragLeave(object? sender, DragEventArgs e) {
        var pos = e.GetPosition(PanelEditorViewPane);
        if (pos.HasValue && _viewModel.GridHelper != null && (pos.Value.X < 0 || pos.Value.Y < 0 || pos.Value.X > _viewModel.GridHelper.ViewWidth || pos.Value.Y > _viewModel.GridHelper.ViewHeight) && _dropZone is not null) {
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

        var width = loc.BoxSize * coordinate.Width;
        var height = loc.BoxSize * coordinate.Height;
        if (loc.XOffset + width > _viewModel?.GridHelper?.ViewWidth || loc.YOffset + height > _viewModel?.GridHelper?.ViewHeight) {
            width = loc.BoxSize;
            height = loc.BoxSize;
        }

        if (_dropZone is null) {
            _dropZone = new DropZone(_viewModel!) { ZIndex = 20 };
            PanelEditorViewPane.SetLayoutBounds(_dropZone, new Rect(loc.XOffset, loc.YOffset, width, height)); // X=50, Y=100, Width=200, Height=200
            PanelEditorViewPane.SetLayoutFlags(_dropZone, AbsoluteLayoutFlags.None);                           // None means the Rectangle properties are absolute values
            PanelEditorViewPane.Children.Add(_dropZone);
        } else {
            PanelEditorViewPane.SetLayoutBounds(_dropZone, new Rect(loc.XOffset, loc.YOffset, width, height)); // X=50, Y=100, Width=200, Height=200
            PanelEditorViewPane.SetLayoutFlags(_dropZone, AbsoluteLayoutFlags.None);                           // None means the Rectangle properties are absolute values
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

    private void DeleteTrackToolbarItem_OnClicked(object? sender, EventArgs e) { }

    private void RotateTrackToolbarItem_OnClicked(object? sender, EventArgs e) { }
    #endregion

    private void Stepper_OnValueChanged(object? sender, ValueChangedEventArgs e) {
        ResizePanelViewArea();
    }

    private void Picker_OnSelectedIndexChanged(object? sender, EventArgs e) {
        if (sender is Picker picker) {
            var selectedIndex = picker.SelectedIndex;
            if (selectedIndex != -1 && selectedIndex < _viewModel.SymbolSets.Count) {
                _viewModel.SetSelectedSet(selectedIndex);
            }
        }
    }

    private void DrawColors() {
        var colors = new List<Color> {
            Colors.Black, Colors.White, Colors.Red, Colors.Green,
            Colors.Blue, Colors.Yellow, Colors.Cyan, Colors.Magenta,
            Colors.Silver, Colors.Gray, Colors.Maroon, Colors.Olive,
            Colors.Purple, Colors.Teal, Colors.Navy, Colors.Lime
        };

        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                var boxView = new BoxView {
                    BackgroundColor = colors[row * 4 + col],
                    WidthRequest    = (double)OnScreenSizeHelpers.Instance.GetScreenSizeValue(10,10,10,10,25,25),
                    HeightRequest   = (double)OnScreenSizeHelpers.Instance.GetScreenSizeValue(10,10,10,10,25,25)
                };
                
                // Adding Drag and Drop recognize
                var dragGesture = new DragGestureRecognizer();
                dragGesture.DragStarting += ColorSelectorOnDragStarting;
                boxView.GestureRecognizers.Add(dragGesture);

                var dropGesture = new DropGestureRecognizer();
                dropGesture.Drop += ColorSelectorOnDrop;
                boxView.GestureRecognizers.Add(dropGesture);

                // Add BoxView to the Grid
                ColorGrid.SetColumn(boxView, col);
                ColorGrid.SetRow(boxView, row);
                ColorGrid.Children.Add(boxView);
            }
        }
    }

    void ColorSelectorOnDragStarting(object? sender, DragStartingEventArgs e) {
        var boxView = (sender as GestureRecognizer)?.Parent as BoxView;
        if (boxView is null) return;
        e.Data.Properties.Add("BackgroundColor", boxView?.BackgroundColor ?? Colors.Black);
        _viewModel.TrackAction = TrackActionEnum.Color;
    }

    void ColorSelectorOnDrop(object? sender, DropEventArgs e) {
        if (e.Data.Properties.TryGetValue("BackgroundColor", out var color)) {
            //var boxView = sender as BoxView;
            //boxView?.BackgroundColor = (Color)color;
            _viewModel.Message = "Color: " + color.ToString() ?? "No Color";
        }
    }
}
*/

    /*
    <BoxView Grid.Column="0" Grid.Row="0" BackgroundColor="#000000" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="0" Grid.Row="1" BackgroundColor="#FFFFFF" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="0" Grid.Row="2" BackgroundColor="#FF0000" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="0" Grid.Row="3" BackgroundColor="#008000" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="1" Grid.Row="0" BackgroundColor="#0000FF" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="1" Grid.Row="1" BackgroundColor="#FFFF00" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="1" Grid.Row="2" BackgroundColor="#00FFFF" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="1" Grid.Row="3" BackgroundColor="#FF00FF" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="2" Grid.Row="0" BackgroundColor="#C0C0C0" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="2" Grid.Row="1" BackgroundColor="#808080" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="2" Grid.Row="2" BackgroundColor="#800000" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="2" Grid.Row="3" BackgroundColor="#808000" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="3" Grid.Row="0" BackgroundColor="#800080" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="3" Grid.Row="1" BackgroundColor="#008080" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="3" Grid.Row="2" BackgroundColor="#000080" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    <BoxView Grid.Column="3" Grid.Row="3" BackgroundColor="#00FF00" WidthRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}" HeightRequest="{markups:OnScreenSize Medium=10, Large=10, ExtraLarge=25, Default=25}"/>
    */
    