using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class TurnoutElementViewModel : ElementViewModel, IElementViewModel {
    public TurnoutElementViewModel(TurnoutPanelElement element, SymbolDetails details) : base(element, details) { }
}
