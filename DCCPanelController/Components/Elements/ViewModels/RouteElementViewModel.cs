using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class RouteElementViewModel : ElementViewModel, IElementViewModel {
    public RouteElementViewModel(RoutePanelElement element, SymbolDetails details) : base(element, details) { }
}
