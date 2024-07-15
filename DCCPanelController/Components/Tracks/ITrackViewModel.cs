using DCCPanelController.Model;

namespace DCCPanelController.Components.Tracks;

public interface ITrackViewModel {
 
    Track Track { get; set; }
    ImageSource Image { get; }
    Rect Bounds { get; set; }
    
    bool IsBusy { get; }
    bool IsNotBusy => !IsBusy;    
    bool IsSelected { get; set; }
    bool IsNotSelected => !IsSelected;
    bool IsRefreshing { get; }
    
}