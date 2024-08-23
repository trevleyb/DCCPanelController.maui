using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.TrackPieces;
using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.ViewModel;

public partial class TrackPieceTestViewModel : BaseViewModel {

    [ObservableProperty] private ITrackPiece _trackPiece;

    public TrackPieceTestViewModel() {
        _trackPiece = new StraightPiece();
    }

    [RelayCommand] public void RotateLeft() => TrackPiece?.RotateLeft();
    [RelayCommand] public void RotateRight() => TrackPiece?.RotateRight();
    [RelayCommand] public void NextState() => TrackPiece?.NextState();
    [RelayCommand] public void PrevState() => TrackPiece?.PrevState();

    [RelayCommand]
    public void NextImage() { }
}

