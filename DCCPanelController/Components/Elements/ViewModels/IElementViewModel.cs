using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public interface IElementViewModel {
 
    IPanelElement Element { get; set; }
    ImageSource Image { get; set; }
    Rect Bounds { get; set; }
    
    bool IsDesignMode { get; set; }
    bool IsBusy { get; }
    bool IsNotBusy => !IsBusy;    
    bool IsSelected { get; set; }
    bool IsNotSelected => !IsSelected;
    bool IsRefreshing { get; }
    
}