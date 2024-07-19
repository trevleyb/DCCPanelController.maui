using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.Elements;
using DCCPanelController.Components.Elements.ViewModels;
using DCCPanelController.Components.Elements.Views;
using DCCPanelController.Helpers;
using DCCPanelController.Model;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private Panel _panel;
    
    [NotifyPropertyChangedFor(nameof(IsPropertyAllowed))]
    [NotifyPropertyChangedFor(nameof(IsTrackSelected))]
    [ObservableProperty] private IElementView? _selectedElement;
    [ObservableProperty] private SymbolViewModel? _selectedSymbol;
    [ObservableProperty] private ObservableCollection<SymbolViewModel> _symbols;
    [ObservableProperty] private ObservableCollection<IElementView> _panelElements = [];
    [ObservableProperty] private ObservableCollection<IElementView> _selectedElements = [];

    [ObservableProperty] private Coordinate _lastCoordinate;
    [ObservableProperty] private TrackActionEnum _trackAction = TrackActionEnum.None;

    [ObservableProperty] private int _minPanelRows = 12;
    [ObservableProperty] private int _minPanelCols = 9;
    [ObservableProperty] private int _maxPanelRows = 36;
    [ObservableProperty] private int _maxPanelCols = 27;
    
    [ObservableProperty] private bool _isPropertyAllowed;
    [ObservableProperty] private bool _multiSelectMode;
    [ObservableProperty] private bool _isTrackSelected;
    [ObservableProperty] private bool _isDropZoneOccupied;
    [ObservableProperty] private bool _isPropertyPanelVisible;

    public GridHelper? GridHelper;

    public PanelEditorViewModel(Panel panel) {
        Symbols = SymbolFactory.AvailableSymbols().ToObservableCollection();
        LastCoordinate = Coordinate.Unreferenced;
        IsPropertyPanelVisible = false;
        Panel = panel;
        Panel.PropertyChanged += PanelOnPropertyChanged;
    }

    private void LoadTrackPlan() {
        PanelElements.Clear();
        foreach (var element in Panel.Elements) {
            var elementView = ElementFactory.GetElementView(element);
            AddElementToPlan(elementView);
        }
    }
    
    private void PanelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(Panel.Cols) or nameof(Panel.Rows) }) {
            UpdatePanelEditorGrid();
        }
    }
    
    /// <summary>
    /// Sets the track that has been selected which will allow functions such as
    /// Rotate, Delete and Properties
    /// </summary>
    public void SetSelectedTrack(IElementView? elementView) {
        if (elementView != null) {
            if (SelectedElements.Contains(elementView)) {
                elementView.ViewModel.IsSelected = false;
                SelectedElements.Remove(elementView);
            } else {
                if (!MultiSelectMode) ClearSelectedTracks();
                elementView.ViewModel.IsSelected = true;
                SelectedElements.Add(elementView);
            }
        } else {
            ClearSelectedTracks();
        }
        IsTrackSelected   = SelectedElements.Any();
        IsPropertyAllowed = SelectedElements.Count == 1;
    }

    public void ClearSelectedTracks() {
        foreach (var element in SelectedElements) element.ViewModel.IsSelected = false;
        SelectedElements.Clear();
        IsPropertyAllowed = false;
        IsTrackSelected   = false;
    }
    
    /// <summary>
    /// Sets the Bounds for the Panel Editor as we need to know this information to re-draw the Panel
    /// </summary>
    /// <param name="width">Width of the View Panel</param>
    /// <param name="height">Height of the View Panel</param>
    public void SetPanelEditorBounds(int width, int height) {
        GridHelper = new GridHelper(width, height, Panel.Cols, Panel.Rows);
        LoadTrackPlan();
    }

    public void UpdatePanelEditorGrid() {
        // First check what the maximum Row and Col is that is currently in use
        // and do not allow the grid to go smaller than this. 
        // var maxCols = Tracks.Max(t => t.Track.);

        CalculateMinGridSize();
        var elements = PanelElements.ToList();
        PanelElements.Clear();
        foreach (var element in elements) {
            AddElementToPlan(element);
        }
    }

    private void CalculateMinGridSize() {
        var minCol = PanelElements.Any() ? PanelElements.Max(x => x.ViewModel.Element.Coordinate.Col + (x.ViewModel.Element.Width-1)) : 12;
        var minRow = PanelElements.Any() ? PanelElements.Max(y => y.ViewModel.Element.Coordinate.Row + (y.ViewModel.Element.Height-1)) : 9;
        MinPanelCols = minCol;
        MinPanelRows = minRow;

        Panel.Cols = Math.Max(Math.Max(Panel.Cols,minCol),12);
        Panel.Rows= Math.Max(Math.Max(Panel.Rows,minRow),9);
        
        // This will force a recalculation of the layout bounds which is used when we 
        // redraw the items on the layout
        // --------------------------------------------------------------------------
        if (GridHelper != null) {
            GridHelper.PanelCols = Panel.Cols;
            GridHelper.PanelRows = Panel.Rows;
        }
    }
    
    /// <summary>
    /// Stores the last coordinates of the item we have dropped so we can use this to store
    /// it to be able to re-draw the item. 
    /// </summary>
    /// <param name="posX">the X-Position of the Drop Location</param>
    /// <param name="posY">the Y-Position of the Drop Location</param>
    /// <returns>The X and Y locations</returns>
    public Coordinate SetLastCoordinates(int posX, int posY) {
        if (GridHelper != null) {
            LastCoordinate = GridHelper.GetGridReference(posX, posY);
            IsDropZoneOccupied = IsCoordinatesOccupied(LastCoordinate);
            return LastCoordinate;
        }
        return Coordinate.Unreferenced;
    }

    public bool IsCoordinatesOccupied(Coordinate? coordinate) {
        if (coordinate == null) return false;
        return PanelElements.Any(x => x.ViewModel.Element.Coordinate.ToString().Equals(coordinate.ToString()));
    }

    [RelayCommand]
    void Validate() {
        ValidateAllProperties();
    }
    
    [RelayCommand]
    public async Task RotateSelectedElement() {
        foreach (var element in SelectedElements) {
            element.ViewModel.Element.Rotation += 90;
            if (element.ViewModel.Element.Rotation >= 360) element.ViewModel.Element.Rotation = 0;
        }
    }

    [RelayCommand]
    public async Task DeleteSelectedElement() {
        var deleteElements = SelectedElements.ToList();
        foreach (var element in deleteElements) {
            PanelElements.Remove(element);
        }
        IsTrackSelected  = false;
        IsPropertyAllowed = false;
        TrackAction = TrackActionEnum.None;
    }

    [RelayCommand]
    public async Task PropertiesSelectedElement() {
        if (IsPropertyPanelVisible) {
            // Manage the Properties of the main Panel
        } else {
            // Manage the other stuff
        }
    }
    
    [RelayCommand]
    public async Task ElementDragAsync(IElementView view) {
        TrackAction = TrackActionEnum.MovingInGrid;
        SelectedElement = view;
    }
    
    [RelayCommand]
    public async Task SymbolDragAsync(SymbolViewModel symbol) {
        ClearSelectedTracks();
        TrackAction = TrackActionEnum.AddingFromToolbox;
        SelectedSymbol = symbol;
    }

    [RelayCommand]
    public async Task ToggleMultiSelectAsync() {
        MultiSelectMode = !MultiSelectMode;
        if (!MultiSelectMode) {
            ClearSelectedTracks();
            IsTrackSelected   = false;
            IsPropertyAllowed = false;
        }
    } 
    
    [RelayCommand]
    public async Task DropAsync(object obj) {
        if (IsCoordinatesOccupied(LastCoordinate)) return;
        
        // Add a track from the toolbox to the Main Grid
        // ----------------------------------------------------------------------------------
        if (TrackAction == TrackActionEnum.AddingFromToolbox && SelectedSymbol is not null) {
            var element = ElementFactory.GetElementView(SelectedSymbol.Name);
            element.ViewModel.Element.Coordinate = LastCoordinate;
            AddElementToPlan(element);
            Panel.Elements.Add(element.ViewModel.Element);
            TrackAction = TrackActionEnum.None;
            SetSelectedTrack(element);
        }

        // Move an item on the Main Grid to another location on the Main Grid
        // ----------------------------------------------------------------------------------
        if (TrackAction == TrackActionEnum.MovingInGrid && SelectedElement is not null) {
            PanelElements.Remove(SelectedElement);
            SelectedElement.ViewModel.Element.Coordinate = LastCoordinate;
            AddElementToPlan(SelectedElement);
            SelectedElement.ViewModel.IsSelected = false;
            TrackAction = TrackActionEnum.None;
        }
    }
    
    private void AddElementToPlan(IElementView view) {
        var gridData = GridHelper?.GetGridCoordinates(view.ViewModel.Element.Coordinate);
        if (gridData is { IsOk: true } gd) {
            view.ViewModel.Bounds = new Rect(gd.XOffset, gd.YOffset, gd.BoxSize, gd.BoxSize);
            PanelElements.Add(view);
        }
        CalculateMinGridSize();
    }

}

public enum TrackActionEnum {
    AddingFromToolbox, 
    MovingInGrid,
    None
}