using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class BlankElementViewModel : ElementViewModelBase, ITrackElementViewModel {
    public BlankElementViewModel() {
        Image = ImageSource.FromFile("angleleft.png");
        Name = "Blank";
    }
}