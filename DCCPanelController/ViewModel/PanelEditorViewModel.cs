using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Components.Tracks;
using DCCPanelController.Components.Tracks.Base;
using DCCPanelController.Components.Tracks.Views;
using DCCPanelController.Helpers;
using DCCPanelController.Model;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private Panel _panel;
    
    [NotifyPropertyChangedFor(nameof(IsPropertyAllowed))]
    [NotifyPropertyChangedFor(nameof(IsTrackSelected))]
    [ObservableProperty] private ITrackViewModel? _selectedTrack;
    [ObservableProperty] private SymbolViewModel? _selectedSymbol;
    [ObservableProperty] private ObservableCollection<SymbolViewModel> _symbols;
    [ObservableProperty] private ObservableCollection<ITrackViewModel> _tracks = [];
    [ObservableProperty] private ObservableCollection<ITrackViewModel> _selectedTracks = [];

    [ObservableProperty] private Coordinate _lastCoordinate;
    [ObservableProperty] private TrackActionEnum _trackAction = TrackActionEnum.None;

    [ObservableProperty] private int _minPanelRows = 12;
    [ObservableProperty] private int _minPanelCols = 9;
    [ObservableProperty] private int _maxPanelRows = 36;
    [ObservableProperty] private int _maxPanelCols = 27;
    
    [ObservableProperty] private bool _isPropertyAllowed = false;
    [ObservableProperty] private bool _multiSelectMode = false;
    [ObservableProperty] private bool _isTrackSelected = false;
    [ObservableProperty] private bool _isDropZoneOccupied = false;
    [ObservableProperty] private bool _isPropertyPanelVisible = false;

    public GridHelper? GridHelper = null;

    public PanelEditorViewModel(Panel panel) {
        Symbols = SymbolFactory.AvailableTracks();
        LastCoordinate = Coordinate.Unreferenced;
        IsPropertyPanelVisible = false;
        Panel = panel;
        Panel.PropertyChanged += PanelOnPropertyChanged;
    }

    private void LoadTrackPlan() {
        Tracks.Clear();
        foreach (var track in Panel.Tracks) {
            var viewModel = CreateTrackViewModel(track);
            AddTrackToPlan(viewModel);
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
    public void SetSelectedTrack(ITrackViewModel? trackView) {
        if (trackView != null) {
            if (SelectedTracks.Contains(trackView)) {
                trackView.IsSelected = false;
                SelectedTracks.Remove(trackView);
            } else {
                if (!MultiSelectMode) ClearSelectedTracks();
                trackView.IsSelected = true;
                SelectedTracks.Add(trackView);
            }
        } else {
            ClearSelectedTracks();
        }
        IsTrackSelected   = SelectedTracks.Any();
        IsPropertyAllowed = SelectedTracks.Count == 1;
    }

    public void ClearSelectedTracks() {
        foreach (var track in SelectedTracks) track.IsSelected = false;
        SelectedTracks.Clear();
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
        RefreshPlanLayoutTracks(Tracks.ToList());
    }

    private void CalculateMinGridSize() {
        var minCol = Tracks.Any() ? Tracks.Max(x => x.Track.Coordinate.Col) : 12;
        var minRow = Tracks.Any() ? Tracks.Max(x => x.Track.Coordinate.Row) : 9;
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
    /// Forces a refresh of all the tracks on the screen. 
    /// </summary>
    private void RefreshPlanLayoutTracks(List<ITrackViewModel> trackPieces) {
        var tracks = trackPieces.ToList();
        Tracks.Clear();
        foreach (var track in tracks) {
            AddTrackToPlan(track);
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
        return Tracks.Any(x => x.Track.Coordinate.ToString().Equals(coordinate.ToString()));
    }

    [RelayCommand]
    void Validate() {
        ValidateAllProperties();
    }
    
    [RelayCommand]
    public async Task RotateSelectedTrack() {
        foreach (var track in SelectedTracks) {
            track.Track.Rotation += 90;
            if (track.Track.Rotation >= 360) track.Track.Rotation = 0;
        }
    }

    [RelayCommand]
    public async Task DeleteSelectedTrack() {
        var deleteTracks = SelectedTracks.ToList();
        foreach (var track in deleteTracks) {
            Tracks.Remove(track);
        }
        IsTrackSelected  = false;
        IsPropertyAllowed = false;
        TrackAction = TrackActionEnum.None;
    }

    [RelayCommand]
    public async Task PropertiesSelectedTrack() {
        if (IsPropertyPanelVisible) {
            // Manage the Properties of the main Panel
        } else {
            // Manage the other stuff
        }
    }

    
    [RelayCommand]
    public async Task TrackDragAsync(TrackView track) {
        TrackAction = TrackActionEnum.MovingInGrid;
        SelectedTrack = track.ViewModel;
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
            var track = new Track() {
                TrackType = SelectedSymbol.TrackType,
                Color = Colors.Black,
                Coordinate = LastCoordinate,
            };
            Panel.Tracks.Add(track);
            var viewModel = CreateTrackViewModel(track);
            AddTrackToPlan(viewModel);
            TrackAction = TrackActionEnum.None;
            SetSelectedTrack(viewModel);
        }

        // Move an item on the Main Grid to another location on the Main Grid
        // ----------------------------------------------------------------------------------
        if (TrackAction == TrackActionEnum.MovingInGrid && SelectedTrack is not null) {
            Tracks.Remove(SelectedTrack);
            SelectedTrack.Track.Coordinate = LastCoordinate;
            AddTrackToPlan(SelectedTrack);
            SelectedTrack.IsSelected = false;
            TrackAction = TrackActionEnum.None;
        }
    }

    private ITrackViewModel CreateTrackViewModel(Track track) {
        var viewModel = TrackViewModelFactory.GetViewModel(track.TrackType);
        viewModel.Track = track;
        return viewModel;
    }
    
    private void AddTrackToPlan(ITrackViewModel viewModel) {
        var gridData = GridHelper?.GetGridCoordinates(viewModel.Track.Coordinate);
        if (gridData is { IsOk: true } gd) {
            viewModel.Bounds = new Rect(gd.XOffset, gd.YOffset, gd.BoxSize, gd.BoxSize);
            Tracks.Add(viewModel);
        }
        CalculateMinGridSize();
    }
    
}

public enum TrackActionEnum {
    AddingFromToolbox, 
    MovingInGrid,
    None
}