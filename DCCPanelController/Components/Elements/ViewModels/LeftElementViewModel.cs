using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class LeftElementViewModel : ElementViewModelBase, ITrackElementViewModel {
    public LeftElementViewModel() {
        Image = ImageSource.FromFile("angleleft.png");
        Name = "Left Track";
    }
}