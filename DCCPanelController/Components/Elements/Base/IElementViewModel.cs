using DCCPanelController.Model;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.Base;

public interface IElementViewModel {
 
    PanelElement Element { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    Coordinate Coordinates { get; set; }
    Rect Bounds { get; set; }
    
    bool IsBusy { get; }
    bool IsNotBusy => !IsBusy;    
    bool IsSelected { get; set; }
    bool IsNotSelected => !IsSelected;
    bool IsRefreshing { get; }
    
}