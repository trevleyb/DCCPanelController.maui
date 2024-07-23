using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class CircleTextElementViewModel : ElementViewModel, IElementViewModel {

    public CircleTextElementViewModel(TextPanelElement element, SymbolDetails details) : base(element, details) { } 
}
