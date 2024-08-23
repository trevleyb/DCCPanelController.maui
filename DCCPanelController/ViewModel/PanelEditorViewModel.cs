using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.Elements;
using DCCPanelController.Components.Elements.Views;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Components.TrackComponents;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Model.Elements;
using DCCPanelController.Model.Elements.Base;
using SymbolViewModel = DCCPanelController.Components.TrackComponents.SymbolViewModel;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private Panel _panel;
    [ObservableProperty] private List<string> _symbolSets = SymbolLoader.Symbols.SetNames;
    [ObservableProperty] private string _selectedSet = "All";
    [ObservableProperty] private ObservableCollection<SymbolViewModel> _filteredSymbols = [];
    
    [NotifyPropertyChangedFor(nameof(IsPropertyAllowed))]
    [NotifyPropertyChangedFor(nameof(IsTrackSelected))]
    [ObservableProperty] private IElementView? _selectedElement;
    [ObservableProperty] private SymbolViewModel? _selectedSymbol;
    [ObservableProperty] private ObservableCollection<IElementView> _panelElements = [];
    [ObservableProperty] private ObservableCollection<IElementView> _selectedElements = [];

    [ObservableProperty] private string _message = "";
    [ObservableProperty] private TrackActionEnum _trackAction = TrackActionEnum.None;

    [ObservableProperty] private int _maxPanelCols = 40;
    [ObservableProperty] private int _minPanelCols = 16;
    [ObservableProperty] private int _minPanelRows = 12;
    [ObservableProperty] private int _maxPanelRows = 30;
    [ObservableProperty] private int _selectedWidth = 1;
    [ObservableProperty] private int _selectedHeight = 1;
    
    [ObservableProperty] private bool _isPropertyAllowed;
    [ObservableProperty] private bool _multiSelectMode;
    [ObservableProperty] private bool _isTrackSelected;
    [ObservableProperty] private bool _isDropZoneOccupied;
    [ObservableProperty] private bool _isPropertyPanelVisible;

    [ObservableProperty] private bool _isTextBlockExpandable;
    [ObservableProperty] private bool _isTextBlockContractable;
    
    [ObservableProperty] private Coordinate _lastCoordinate;
    [ObservableProperty] private int _lastZIndex;
    [ObservableProperty] private int _span;
    
    public GridHelper? GridHelper;

    public PanelEditorViewModel(Panel panel) {
        SetSelectedSet(0);
        LastCoordinate = Coordinate.Unreferenced;
        IsPropertyPanelVisible = false;
        Panel = panel;
        Panel.PropertyChanged += PanelOnPropertyChanged;
        Span = 2;
    }

    private void LoadTrackPlan() {
        PanelElements.Clear();
        foreach (var element in Panel.Elements) {
            var view = ElementFactory.CreateElementView(element);
            if (view is not null) AddElementToPlan(view);
        }
    }

    #region Manage the locations and changes in the side of the layout
    private void PanelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(Panel.Cols) or nameof(Panel.Rows) }) {
            UpdatePanelEditorGrid();
        }
    }
    
    /// <summary>
    /// Stores the last coordinates of the item we have dropped so we can use this to store
    /// it to be able to re-draw the item. 
    /// </summary>
    /// <returns>The X and Y locations</returns>
    public Coordinate SetLastCoordinates(int posX, int posY) {
        if (GridHelper != null) {
            LastCoordinate = GridHelper.GetGridReference(posX, posY, SelectedWidth, SelectedHeight, LastZIndex);
            IsDropZoneOccupied = TrackAction switch {
                TrackActionEnum.None              => false,
                TrackActionEnum.Color             => false,
                TrackActionEnum.AddingFromToolbox => IsCellOccupied(LastCoordinate, SelectedElement?.ViewModel.Element),
                TrackActionEnum.MovingInGrid      => IsCellOccupied(LastCoordinate, SelectedElement?.ViewModel.Element),
                _                                 => false,
            };
            if (IsOutsideBounds(LastCoordinate)) IsDropZoneOccupied = true;
            return LastCoordinate;
        }
        return Coordinate.Unreferenced;
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
        // Check what the maximum Row and Col is that is currently in use
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
        var minCol = PanelElements.Any() ? PanelElements.Max(x => x.ViewModel.Element.Coordinate.Col + (x.ViewModel.Element.Coordinate.Width-1)) : MinPanelCols ;
        var minRow = PanelElements.Any() ? PanelElements.Max(y => y.ViewModel.Element.Coordinate.Row + (y.ViewModel.Element.Coordinate.Height-1)) : MinPanelRows;
        MinPanelCols = minCol;
        MinPanelRows = minRow;

        Panel.Cols = Math.Max(Math.Max(Panel.Cols,minCol),MinPanelCols);
        Panel.Rows= Math.Max(Math.Max(Panel.Rows,minRow),MinPanelRows);
        
        // This will force a recalculation of the layout bounds which is used when we 
        // redraw the items on the layout
        // --------------------------------------------------------------------------
        if (GridHelper != null) {
            GridHelper.PanelCols = Panel.Cols;
            GridHelper.PanelRows = Panel.Rows;
        }
    }
    #endregion

    /// <summary>
    /// This looks to see if the coordinates provided are currently occupied already.
    /// This looks at the Width and Height of each item in the panel and returns true
    /// if any of them clash with the provided coordinate. 
    /// </summary>
    public bool IsCellOccupied(Coordinate coordinates, IPanelElement? activeElement) {
        foreach (var element in Panel.Elements) {
            if (activeElement is null || element != activeElement) {
                for (var coordX = 0; coordX < coordinates.Width; coordX++) {
                    for (var coordY = 0; coordY < coordinates.Height; coordY++) {
                        for (var elementX = 0; elementX < element.Coordinate.Width; elementX++) {
                            for (var elementY = 0; elementY < element.Coordinate.Height; elementY++) {
                                
                                // if the same Col,Row is occupied and the ZIndex >= the selected one 
                                // this allows things like a Text Box or Circle to sit over the top of another item
                                // --------------------------------------------------------------------------------
                                if (element.Coordinate.Col + elementX == coordinates.Col + coordX && 
                                    element.Coordinate.Row + elementY == coordinates.Row + coordY) {
                                    if (element.Coordinate.ZIndex >= coordinates.ZIndex) {
                                        Message = $"Coordinate {coordinates.Col},{coordinates.Row},{element.Coordinate.Width},{element.Coordinate.Height} is Occupied";
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    } 

    /// <summary>
    /// Works out if the coordinates provided, plus any height/width of the element are inside or outside
    /// the bounds of the Panel. Will return true if it is outside the bounds. 
    /// </summary>
    public bool IsOutsideBounds(Coordinate coordinates) {
        var isOutOfBounds = (coordinates.Col + (coordinates.Width- 1) > GridHelper?.PanelCols || coordinates.Row + (coordinates.Height -1) > GridHelper?.PanelRows);
        if (isOutOfBounds) Message = $"{coordinates.Col}:{coordinates.Width},{coordinates.Row}:{coordinates.Height} => {GridHelper?.PanelCols} x {GridHelper?.PanelRows} Out of Bounds";
        return isOutOfBounds;
    } 
    
    #region Manage the selected elements
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

        IsTrackSelected = SelectedElements.Any();
        IsPropertyAllowed = SelectedElements.Count == 1;
        SetTextOptions();
    }

    private void SetTextOptions() {
        var allCanExpand = true;
        var allCanContract = true;
        var foundTextBlock = false;
        foreach (var element in SelectedElements) {
            if (element is TextElementView elementView) {
                var coord = elementView.ViewModel.Element.Coordinate;
                if (coord.Width <= 1) allCanContract = false;
                if (coord.Col + (coord.Width - 1) >= GridHelper?.PanelCols) allCanExpand = false;
                foundTextBlock = true;
            }
        }
        IsTextBlockContractable = foundTextBlock && allCanContract;
        IsTextBlockExpandable = foundTextBlock && allCanExpand;
    }

    public void ClearSelectedTracks() {
        foreach (var element in SelectedElements) element.ViewModel.IsSelected = false;
        SelectedElements.Clear();
        IsPropertyAllowed = false;
        IsTrackSelected   = false;
    }
    #endregion
    
    [RelayCommand]
    public async Task Save() {
        try {
            IsBusy = true;
            App.ServiceProvider?.GetService<PanelsViewModel>()?.Save();
        } finally {
            IsBusy = false;
        }
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
            if (element.ViewModel.Element.Coordinate.Height != element.ViewModel.Element.Coordinate.Width) {
                var width = element.ViewModel.Element.Coordinate.Width;
                var height = element.ViewModel.Element.Coordinate.Height;
                element.ViewModel.Element.Coordinate.Width = height;
                element.ViewModel.Element.Coordinate.Height = width;
            }
            UpdateElementOnPlan(element);
        }
    }

    [RelayCommand]
    public async Task DeleteSelectedElement() {
        var deleteElements = SelectedElements.ToList();
        foreach (var element in deleteElements) {
            Panel.Elements.Remove(element.ViewModel.Element);
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
    public async Task ExpandTextSize() {
        foreach (var element in SelectedElements) {
            if (element is TextElementView) {
                // TODO: Add support that if it is rotated 90/270 then it is Height that changes
                element.ViewModel.Element.Coordinate.Width++;
                if (element.ViewModel.Element.Coordinate.Width > GridHelper?.PanelCols) element.ViewModel.Element.Coordinate.Width = GridHelper?.PanelCols ?? 1;
                UpdateElementOnPlan(element);
            }
        }
    }

    [RelayCommand]
    public async Task ContractTextSize() {
        foreach (var element in SelectedElements) {
            if (element is TextElementView) {
                // TODO: Add support that if it is rotated 90/270 then it is Height that changes
                element.ViewModel.Element.Coordinate.Width--;
                if (element.ViewModel.Element.Coordinate.Width < 1) element.ViewModel.Element.Coordinate.Width = 1;
                UpdateElementOnPlan(element);
            }
        }
    }

    [RelayCommand]
    public async Task ElementDragAsync(IElementView view) {
        TrackAction = TrackActionEnum.MovingInGrid;
        SelectedElement = view;
        SelectedWidth   = view.ViewModel.Element.Coordinate.Width;
        SelectedHeight  = view.ViewModel.Element.Coordinate.Height;
        LastZIndex      = view.ViewModel.Element.Coordinate.ZIndex;
    }
    
    [RelayCommand]
    public async Task SymbolDragAsync(SymbolViewModel symbol) {
        ClearSelectedTracks();
        TrackAction = TrackActionEnum.AddingFromToolbox;
        SelectedSymbol = symbol;
        SelectedWidth  = symbol.Width;
        SelectedHeight = symbol.Height;
        LastZIndex     = symbol.ZIndex;
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
       
        // Add a track from the toolbox to the Main Grid
        // ----------------------------------------------------------------------------------
        if (TrackAction == TrackActionEnum.AddingFromToolbox && SelectedSymbol is not null) {
            if (IsCellOccupied(LastCoordinate, null)) return;
            var elementView = ElementFactory.CreateElementView(SelectedSymbol.Key);
            if (elementView is not null) {
                elementView.ViewModel.Element.Coordinate = LastCoordinate;
                elementView.ViewModel.Element.Coordinate.ZIndex = SelectedSymbol.ZIndex;
                AddElementToPlan(elementView);
                Panel.Elements.Add(elementView.ViewModel.Element);
                TrackAction = TrackActionEnum.None;
                SetSelectedTrack(elementView);
            }
        }

        // Apply the Color if the Drop is a Valid Element
        // ----------------------------------------------
        if (TrackAction == TrackActionEnum.Color) {
            Message = "We got here";
        }
        
        // Move an item on the Main Grid to another location on the Main Grid
        // ----------------------------------------------------------------------------------
        if (TrackAction == TrackActionEnum.MovingInGrid && SelectedElement is not null) {
            if (IsCellOccupied(LastCoordinate, SelectedElement.ViewModel.Element)) return;
            PanelElements.Remove(SelectedElement);
            SelectedElement.ViewModel.Element.Coordinate = LastCoordinate;
            AddElementToPlan(SelectedElement);
            SetSelectedTrack(SelectedElement);
            TrackAction = TrackActionEnum.None;
        }
        LastZIndex = 0;
        LastCoordinate = Coordinate.Unreferenced;
    }

    private void RecalculateBounds(IElementView view) {
        var gridData = GridHelper?.GetGridCoordinates(view.ViewModel.Element.Coordinate);
        if (gridData is { IsOk: true } gd) {
            view.ViewModel.Bounds = new Rect(gd.XOffset, gd.YOffset, gd.BoxSize * view.ViewModel.Element.Coordinate.Width, gd.BoxSize * view.ViewModel.Element.Coordinate.Height);
        }
        CalculateMinGridSize();
    }
    
    private void UpdateElementOnPlan(IElementView view) {
        PanelElements.Remove(view);
        AddElementToPlan(view);
    }

    private void AddElementToPlan(IElementView view) {
        RecalculateBounds(view);
        PanelElements.Add(view);
    }

    public void SetSelectedSet(int index) {
        if (index >= 0 && index < SymbolSets.Count) {
            SelectedSet = SymbolSets[index];
            FilteredSymbols.Clear();
            FilteredSymbols = SymbolFactory.AvailableSymbols(SelectedSet).ToObservableCollection();
        }
    }
}

public enum TrackActionEnum {
    AddingFromToolbox, 
    MovingInGrid,
    Color,
    None
}