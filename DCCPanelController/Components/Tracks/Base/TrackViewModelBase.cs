using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;

namespace DCCPanelController.Components.Tracks.Base;

public partial class TrackViewModelBase : BaseViewModel, ITrackViewModel {
    
    [ObservableProperty] 
    private string _name = string.Empty;

    [ObservableProperty] 
    private ImageSource? _image;
    
    [ObservableProperty] 
    private ImageSource? _text;

    [ObservableProperty]
    private Track _track;
    
    [ObservableProperty] 
    private Rect _bounds;

}
