using DCCPanelController.Model;

namespace DCCPanelController.Symbols.TrackViewModels;

public interface ITrackViewModel {
 
    string Name { get; }
    ImageSource Image { get; }
    TrackPiece Track { get; set; }
    Rect Bounds { get; set; }
    
    bool IsBusy { get; }
    bool IsNotBusy => !IsBusy;    
    bool IsSelected { get; set; }
    bool IsNotSelected => !IsSelected;
    bool IsRefreshing { get; }
    
}