using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Symbols;
using DCCPanelController.Symbols.Tracks;
using DCCPanelController.Symbols.TrackViewModels;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {
    public readonly int MaxColumns = 24;
    public readonly int MaxRows = 18;

    [NotifyPropertyChangedFor(nameof(IsPropertyAllowed))]
    [NotifyPropertyChangedFor(nameof(IsTrackSelected))]
    [ObservableProperty] private ObservableCollection<ITrackViewModel> _selectedTracks;
    [ObservableProperty] private ITrackViewModel? _selectedTrack;
    [ObservableProperty] private SymbolViewModel? _selectedSymbol;
    [ObservableProperty] private ObservableCollection<SymbolViewModel> _symbols;
    [ObservableProperty] private ObservableCollection<ITrackViewModel> _tracks;
    
    [ObservableProperty] private string _lastCoordinate = string.Empty;
    [ObservableProperty] private TrackActionEnum _trackAction = TrackActionEnum.None;
    
    [ObservableProperty] private bool _isPropertyAllowed = false;
    [ObservableProperty] private bool _multiSelectMode = false;
    [ObservableProperty] private bool _isTrackSelected = false;
    [ObservableProperty] private bool _isDropZoneOccupied = false;
    
    public GridHelper? GridHelper = null;

    public PanelEditorViewModel() {
        Symbols = BuildSymbolsList();
        Tracks = new ObservableCollection<ITrackViewModel>();
        SelectedTracks = new ObservableCollection<ITrackViewModel>();
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
        GridHelper = new GridHelper(width, height, MaxColumns, MaxRows);
    }
    
    /// <summary>
    /// Stores the last coordinates of the item we have dropped so we can use this to store
    /// it to be able to re-draw the item. 
    /// </summary>
    /// <param name="posX">the X-Position of the Drop Location</param>
    /// <param name="posY">the Y-Position of the Drop Location</param>
    /// <returns>The X and Y locations</returns>
    public GridData SetLastCoordinates(int posX, int posY) {
        LastCoordinate = GridHelper?.GetGridReference(posX, posY) ?? "";
        IsDropZoneOccupied = IsCoordinatesOccupied(LastCoordinate);    
        return GridHelper?.GetGridCoordinates(LastCoordinate) ?? GridData.Error();
    }

    public bool IsCoordinatesOccupied(int posX, int posY) {
        var gridRef = GridHelper?.GetGridReference(posX, posY) ?? "";
        return IsCoordinatesOccupied(gridRef);
    }
    
    public bool IsCoordinatesOccupied(string gridRef) {
        return Tracks.Any(x => x.Track.Coordinate.Equals(gridRef));
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
           
            var viewModel = TrackViewModelFactory.GetViewModel(SelectedSymbol.TrackType);
            viewModel.Track = new TrackPiece() {
                Color = Colors.Black,
                Coordinate = LastCoordinate,
            };

            var gridData = GridHelper?.GetGridCoordinates(LastCoordinate);
            if (gridData is { IsOk: true } gd) {
                viewModel.Bounds = new Rect(gd.XOffset, gd.YOffset, gd.BoxSize, gd.BoxSize);
                Tracks.Add(viewModel);
            }
            TrackAction = TrackActionEnum.None;
        }

        // Move an item on the Main Grid to another location on the Main Grid
        // ----------------------------------------------------------------------------------
        if (TrackAction == TrackActionEnum.MovingInGrid && SelectedTrack is not null) {
            Tracks.Remove(SelectedTrack);

            // Update the new Coordinates (Last Coordinate should be set by the Dragging)
            // --------------------------------------------------------------------------
            SelectedTrack.Track.Coordinate = LastCoordinate;
            var gridData = GridHelper?.GetGridCoordinates(LastCoordinate);
            if (gridData is { IsOk: true } gd) {
                SelectedTrack.Bounds = new Rect(gd.XOffset, gd.YOffset, gd.BoxSize, gd.BoxSize);
                Tracks.Add(SelectedTrack);
                SelectedTrack.IsSelected = false;
            }
            TrackAction = TrackActionEnum.None;
        }
    }
}

public enum TrackActionEnum {
    AddingFromToolbox, 
    MovingInGrid,
    None
}