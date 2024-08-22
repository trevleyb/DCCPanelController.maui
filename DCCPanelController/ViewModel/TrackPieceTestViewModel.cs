using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.TrackImages;
using DCCPanelController.Components.Tracks;
using TrackImage = DCCPanelController.Components.TrackImages.TrackImage;

namespace DCCPanelController.ViewModel;

public partial class TrackPieceTestViewModel : BaseViewModel {

    [ObservableProperty] private List<TrackPiece> _tracks;
    [ObservableProperty] private int _scale = 10;
    [ObservableProperty] private int _componentWidth = 48;
    [ObservableProperty] private int _componentHeight = 48;
    [ObservableProperty] private int _rotation = 0;
    [ObservableProperty] private string _label;

    public TrackPieceTestViewModel() {
    }
    
}

