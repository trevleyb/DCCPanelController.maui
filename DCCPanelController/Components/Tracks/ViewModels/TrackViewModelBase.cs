using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;

namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class TrackViewModelBase : BaseViewModel, ITrackViewModel {
    
    [ObservableProperty] 
    private string _name = string.Empty;

    [ObservableProperty] 
    private ImageSource? _image;
    
    [ObservableProperty]
    private Track _track;
    
    [ObservableProperty] 
    private Rect _bounds;

}
