using DCCPanelController.Components.TrackComponents;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class RouteElementViewModel : ElementViewModel, IElementViewModel {
    public RouteElementViewModel(RoutePanelElement element, SymbolDetails details) : base(element, details) { }
}
