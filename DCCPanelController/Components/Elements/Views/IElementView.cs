using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.Views;

public interface IElementView : IView {
    IElementViewModel ViewModel { get; init; }
}