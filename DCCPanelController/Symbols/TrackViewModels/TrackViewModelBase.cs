using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;

namespace DCCPanelController.Symbols.TrackViewModels;

public partial class TrackViewModelBase : BaseViewModel, ITrackViewModel {
    
    [ObservableProperty] 
    private string _name = string.Empty;

    [ObservableProperty] 
    private ImageSource? _image;
    
    [ObservableProperty]
    private TrackPiece _track;
    
    [ObservableProperty] 
    private Rect _bounds;

}
