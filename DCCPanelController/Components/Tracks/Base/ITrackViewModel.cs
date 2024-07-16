using DCCPanelController.Model;

namespace DCCPanelController.Components.Tracks.Base;

public interface ITrackViewModel {
 
    Track Track { get; set; }
    string Text { get; set; }
    ImageSource Image { get; set; }
    Rect Bounds { get; set; }
    
    bool IsBusy { get; }
    bool IsNotBusy => !IsBusy;    
    bool IsSelected { get; set; }
    bool IsNotSelected => !IsSelected;
    bool IsRefreshing { get; }
    
}