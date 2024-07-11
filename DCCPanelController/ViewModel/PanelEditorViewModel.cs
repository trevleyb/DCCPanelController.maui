using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Symbols;
using DCCPanelController.Symbols.Tracks;
using DCCPanelController.Symbols.TrackViewModels;
using SixLabors.Fonts.WellKnownIds;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private string _panelName;
    [ObservableProperty] private string _panelId;
    
    [NotifyPropertyChangedFor(nameof(IsPropertyAllowed))]
    [NotifyPropertyChangedFor(nameof(IsTrackSelected))]
    [ObservableProperty] private ObservableCollection<ITrackViewModel> _selectedTracks;
    [ObservableProperty] private ITrackViewModel? _selectedTrack;
    [ObservableProperty] private SymbolViewModel? _selectedSymbol;
    [ObservableProperty] private ObservableCollection<SymbolViewModel> _symbols;
    [ObservableProperty] private ObservableCollection<ITrackViewModel> _tracks;

    [ObservableProperty] private Coordinate _lastCoordinate;
    [ObservableProperty] private TrackActionEnum _trackAction = TrackActionEnum.None;

    [NotifyPropertyChangedFor(nameof(PanelRatio))]
    [ObservableProperty] private int _panelRows;
    [NotifyPropertyChangedFor(nameof(PanelRatio))]
    [ObservableProperty] private int _panelCols;

    [ObservableProperty] private int _minPanelRows;
    [ObservableProperty] private int _minPanelCols;
    [ObservableProperty] private int _maxPanelRows;
    [ObservableProperty] private int _maxPanelCols;
    
    [ObservableProperty] private bool _isPropertyAllowed = false;
    [ObservableProperty] private bool _multiSelectMode = false;
    [ObservableProperty] private bool _isTrackSelected = false;
    [ObservableProperty] private bool _isDropZoneOccupied = false;

    public string PanelRatio => CalculateRatio(PanelCols, PanelRows); 
    public GridHelper? GridHelper = null;

    public PanelEditorViewModel() {
        Symbols = BuildSymbolsList();
        Tracks = new ObservableCollection<ITrackViewModel>();
        SelectedTracks = new ObservableCollection<ITrackViewModel>();
        PanelRows = 18; // Needs to get loaded when we reload the panel 
        PanelCols = 24; // Needs to get loaded when we reload the panel
        MinPanelCols = 12;
        MinPanelRows = 9;
        MaxPanelCols = 36;
        MaxPanelRows = 27;
        LastCoordinate = Coordinate.Unreferenced;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(PanelCols) or nameof(PanelRows) }) {
            UpdatePanelEditorGrid();
        }
    }

    /// <summary>
    /// Build up the Toolbar Icons that will allow drag onto the main canvas
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<SymbolViewModel> BuildSymbolsList() {
        var symbols = new ObservableCollection<SymbolViewModel> {
            new() { TrackType = TrackTypesEnum.StraightTrack,   Name = "Straight",   Image = ImageSource.FromFile("straight.png") },
            new() { TrackType = TrackTypesEnum.Terminator,      Name = "Terminate",  Image = ImageSource.FromFile("terminate.png") },
            new() { TrackType = TrackTypesEnum.Crossing,        Name = "Crossing",   Image = ImageSource.FromFile("crossing.png") },
            new() { TrackType = TrackTypesEnum.LeftTrack,       Name = "Left",       Image = ImageSource.FromFile("angleleft.png") },
            new() { TrackType = TrackTypesEnum.RightTrack,      Name = "Right",      Image = ImageSource.FromFile("angleright.png") },
            new() { TrackType = TrackTypesEnum.LeftTurnout,     Name = "Turnout(L)", Image = ImageSource.FromFile("turnoutleft.png") },
            new() { TrackType = TrackTypesEnum.RightTurnout,    Name = "Turnout(R)", Image = ImageSource.FromFile("turnoutright.png") },
            new() { TrackType = TrackTypesEnum.WyeJunction,     Name = "Wye-Junction", Image = ImageSource.FromFile("yjunction.png") },
            new() { TrackType = TrackTypesEnum.StraightTrack,   Name = "Straight",   Image = ImageSource.FromFile("straight.png") },
            new() { TrackType = TrackTypesEnum.Terminator,      Name = "Terminate",  Image = ImageSource.FromFile("terminate.png") },
            new() { TrackType = TrackTypesEnum.Crossing,        Name = "Crossing",   Image = ImageSource.FromFile("crossing.png") },
            new() { TrackType = TrackTypesEnum.LeftTrack,       Name = "Left",       Image = ImageSource.FromFile("angleleft.png") },
            new() { TrackType = TrackTypesEnum.RightTrack,      Name = "Right",      Image = ImageSource.FromFile("angleright.png") },
            new() { TrackType = TrackTypesEnum.LeftTurnout,     Name = "Turnout(L)", Image = ImageSource.FromFile("turnoutleft.png") },
            new() { TrackType = TrackTypesEnum.RightTurnout,    Name = "Turnout(R)", Image = ImageSource.FromFile("turnoutright.png") },
            new() { TrackType = TrackTypesEnum.WyeJunction,     Name = "Wye-Junction", Image = ImageSource.FromFile("yjunction.png") },
            new() { TrackType = TrackTypesEnum.StraightTrack,   Name = "Straight",   Image = ImageSource.FromFile("straight.png") },
        new() { TrackType = TrackTypesEnum.Terminator,      Name = "Terminate",  Image = ImageSource.FromFile("terminate.png") },
        new() { TrackType = TrackTypesEnum.Crossing,        Name = "Crossing",   Image = ImageSource.FromFile("crossing.png") },
        new() { TrackType = TrackTypesEnum.LeftTrack,       Name = "Left",       Image = ImageSource.FromFile("angleleft.png") },
        new() { TrackType = TrackTypesEnum.RightTrack,      Name = "Right",      Image = ImageSource.FromFile("angleright.png") },
        new() { TrackType = TrackTypesEnum.LeftTurnout,     Name = "Turnout(L)", Image = ImageSource.FromFile("turnoutleft.png") },
        new() { TrackType = TrackTypesEnum.RightTurnout,    Name = "Turnout(R)", Image = ImageSource.FromFile("turnoutright.png") },
        new() { TrackType = TrackTypesEnum.WyeJunction,     Name = "Wye-Junction", Image = ImageSource.FromFile("yjunction.png") }

        };
        return symbols;
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
            IsTrackSelected   = SelectedTracks.Any();
            IsPropertyAllowed = SelectedTracks.Count == 1;
        }
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
        GridHelper = new GridHelper(width, height, PanelCols, PanelRows);
    }
    
    public void UpdatePanelEditorGrid() {
        // First check what the maximum Row and Col is that is currently in use
        // and do not allow the grid to go smaller than this. 
        // var maxCols = Tracks.Max(t => t.Track.);
        
        CalculateMinGridSize();
        RefreshPlanLayoutTracks(Tracks.ToList());
    }

    private void CalculateMinGridSize() {
        var minCol = Tracks.Any() ? Tracks.Max(x => x.Track.Coordinate.Column) : 12;
        var minRow = Tracks.Any() ? Tracks.Max(x => x.Track.Coordinate.Row) : 9;
        MinPanelCols = minCol;
        MinPanelRows = minRow;

        PanelCols = Math.Max(Math.Max(PanelCols,minCol),12);
        PanelRows = Math.Max(Math.Max(PanelRows,minRow),9);
        
        // This will force a recalculation of the layout bounds which is used when we 
        // redraw the items on the layout
        // --------------------------------------------------------------------------
        if (GridHelper != null) {
            GridHelper.PanelCols = PanelCols;
            GridHelper.PanelRows = PanelRows;
        }
    }

    /// <summary>
    /// Forces a refresh of all the tracks on the screen. 
    /// </summary>
    private void RefreshPlanLayoutTracks(List<ITrackViewModel> trackPieces) {
        var tracks = trackPieces.Select(x => x);
        Tracks.Clear();
        foreach (var track in tracks) {
            var viewModel = CreateTrackViewModel(track.Track.TrackType, track.Track.Coordinate);
            AddTrackToPlan(viewModel,track.Track.Coordinate);
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
    }

    
    [RelayCommand]
    public async Task TrackDragAsync(TrackView track) {
        TrackAction = TrackActionEnum.MovingInGrid;
        SelectedTrack = track.ViewModel;
    }
    
    [RelayCommand]
    public async Task SymbolDragAsync(SymbolViewModel symbol) {
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
            var viewModel = CreateTrackViewModel(SelectedSymbol.TrackType, LastCoordinate);
            AddTrackToPlan(viewModel,LastCoordinate);
            TrackAction = TrackActionEnum.None;
        }

        // Move an item on the Main Grid to another location on the Main Grid
        // ----------------------------------------------------------------------------------
        if (TrackAction == TrackActionEnum.MovingInGrid && SelectedTrack is not null) {
            Tracks.Remove(SelectedTrack);
            SelectedTrack.Track.Coordinate = LastCoordinate;
            AddTrackToPlan(SelectedTrack, LastCoordinate);
            SelectedTrack.IsSelected = false;
            TrackAction = TrackActionEnum.None;
        }
    }

    private ITrackViewModel CreateTrackViewModel(TrackTypesEnum trackType, Coordinate coordinate) {
        var viewModel = TrackViewModelFactory.GetViewModel(trackType);
        viewModel.Track = new TrackPiece() {
            Color = Colors.Black,
            Coordinate = coordinate,
        };
        return viewModel;
    }

    private void AddTrackToPlan(ITrackViewModel viewModel, Coordinate coordinate) {
        var gridData = GridHelper?.GetGridCoordinates(coordinate);
        if (gridData is { IsOk: true } gd) {
            viewModel.Bounds = new Rect(gd.XOffset, gd.YOffset, gd.BoxSize, gd.BoxSize);
            Tracks.Add(viewModel);
        }
        CalculateMinGridSize();
    }
    
    public static string CalculateRatio(int col, int row) {
        double GCD(double a, double b) {
            while (b != 0) {
                double temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        var gcd = GCD(col, row);
        var X = col / gcd;
        var Y = row / gcd;
        return $"{X:0.#}:{Y:0.#}";
    }
}

public enum TrackActionEnum {
    AddingFromToolbox, 
    MovingInGrid,
    None
}