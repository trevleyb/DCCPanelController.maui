using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public interface ITextElementViewModel : IElementViewModel {
    string Name { get; set; }
    string Text { get; set; }
}