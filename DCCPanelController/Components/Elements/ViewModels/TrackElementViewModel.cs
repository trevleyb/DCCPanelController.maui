using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.TrackComponents;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class TrackElementViewModel : ElementViewModel, IElementViewModel {

    public TrackElementViewModel(TrackPanelElement element, SymbolDetails details) : base(element, details) { } 
}
