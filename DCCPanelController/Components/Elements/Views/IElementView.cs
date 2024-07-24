using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public interface IElementView : IView {
    IElementViewModel ViewModel { get; }
}