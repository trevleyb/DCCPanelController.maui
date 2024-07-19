using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public interface ITrackElementViewModel : IElementViewModel {
    string Name { get; set; }
    ImageSource? Image { get; set; }
}