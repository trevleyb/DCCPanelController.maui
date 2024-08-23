using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.TrackPieces;
using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.ViewModel;

public partial class TrackPieceTestViewModel : BaseViewModel {

    [ObservableProperty] private ITrackPiece _trackPiece;

    private int currentPiece = 0;
    private List<ITrackPiece> _trackPieces = [];

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
    public void NextImage() {
        currentPiece++;
        if (currentPiece >= _trackPieces.Count) currentPiece = 0;
        TrackPiece = _trackPieces[currentPiece];
        OnPropertyChanged(nameof(TrackPiece));
    }
}

