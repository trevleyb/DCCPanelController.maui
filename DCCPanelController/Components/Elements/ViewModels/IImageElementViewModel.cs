using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public interface IImageElementViewModel : IElementViewModel {
    string Name { get; set; }
    Image Image { get; set; }
}