using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.TrackPieces;
using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.ViewModel;

public partial class TrackPieceTestViewModel : BaseViewModel {

    [ObservableProperty] private ITrackPiece _trackPiece;
    [ObservableProperty] private ITrackPiece? _overlay;

    private int _currentPiece = 0;
    private readonly List<ITrackPiece> _trackPieces = [];

    public TrackPieceTestViewModel() {
        _trackPieces.Add(new TrackStraight());
        _trackPieces.Add(new TrackCorner());
        _trackPieces.Add(new TrackCrossing());
        _trackPieces.Add(new TrackTerminator());
        _trackPieces.Add(new TrackLeftTurnout());
        _trackPieces.Add(new TrackRightTurnout());
        _trackPieces.Add(new TrackThreeway());
        _trackPieces.Add(new TrackStraightContinuation());
        _trackPieces.Add(new TrackCornerContinuation());
        TrackPiece = _trackPieces[0];
    }

    [RelayCommand] public void RotateLeft() => TrackPiece?.RotateLeft();
    [RelayCommand] public void RotateRight() => TrackPiece?.RotateRight();
    [RelayCommand] public void NextState() => TrackPiece?.NextState();
    [RelayCommand] public void PrevState() => TrackPiece?.PrevState();

    [RelayCommand]
    public void AddButton() {
        if (Overlay != null) {
            Overlay = null;
        } else {
            var button = new TrackButton();
            //button.SetCompassPoints(TrackPiece);
            Overlay = button;
        }
    }

    [RelayCommand]
    public void AddLabel() {
        if (Overlay != null) {
            Overlay = null;
        } else {
            var label = new TrackLabel();
            //compass.SetCompassPoints(TrackPiece);
            Overlay = label;
        }
    }

    [RelayCommand]
    public void AddCompass() {
        if (Overlay != null) {
            Overlay = null;
        } else {
            var compass = new TrackCompass();
            compass.SetCompassPoints(TrackPiece);
            Overlay = compass;
        }
    }

    [RelayCommand]
    public void NextImage() {
        _currentPiece++;
        if (_currentPiece >= _trackPieces.Count) _currentPiece = 0;
        TrackPiece = _trackPieces[_currentPiece];
        OnPropertyChanged(nameof(TrackPiece));
    }
}

