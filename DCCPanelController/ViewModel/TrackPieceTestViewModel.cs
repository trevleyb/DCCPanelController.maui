using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.TrackPieces;
using DCCPanelController.Components.TrackPieces.Base;
using DCCPanelController.Components.TrackPieces.SVGManager;

namespace DCCPanelController.ViewModel;

public partial class TrackPieceTestViewModel : BaseViewModel {

    [ObservableProperty] private ITrackPiece _trackPiece;
    [ObservableProperty] private ITrackPiece? _overlay;
    [ObservableProperty] private List<string> _styles;
    
    private bool isCompassOn = false;
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
        Styles = SvgStyles.AvailableStyles;
        
    }

    [RelayCommand]
    public void RotateLeft() {
        TrackPiece?.RotateLeft();
        if (isCompassOn) SetCompassOverlay();
    }

    [RelayCommand] public void RotateRight() {
        TrackPiece?.RotateRight();
        if (isCompassOn) SetCompassOverlay();
    }

    [RelayCommand]
    public void NextState() {
        TrackPiece?.NextState();
        if (isCompassOn) SetCompassOverlay();
    }

    [RelayCommand]
    public void PrevState() {
        TrackPiece?.PrevState();
        if (isCompassOn) SetCompassOverlay();
    }

    [RelayCommand]
    public void AddButton() {
        if (Overlay != null) {
            Overlay = null;
        } else {
            var button = new TrackButton();
            Overlay = button;
            isCompassOn = false;
        }
    }

    [RelayCommand]
    public void AddLabel() {
        if (Overlay != null) {
            Overlay = null;
        } else {
            var label = new TrackLabel();
            Overlay = label;
            isCompassOn = false;
        }
    }

    [RelayCommand]
    public void AddCompass() {
        if (Overlay != null) {
            Overlay = null;
            isCompassOn = false;
        } else {
            SetCompassOverlay();
        }
    }

    private void SetCompassOverlay() {
        Overlay = null;
        var compass = new TrackCompass();
        compass.SetCompassPoints(TrackPiece);
        Overlay = compass;
        isCompassOn = true;
    }
    
    [RelayCommand]
    public void NextImage() {
        _currentPiece++;
        if (_currentPiece >= _trackPieces.Count) _currentPiece = 0;
        TrackPiece = _trackPieces[_currentPiece];
        if (isCompassOn) SetCompassOverlay();
        OnPropertyChanged(nameof(TrackPiece));
    }
}

