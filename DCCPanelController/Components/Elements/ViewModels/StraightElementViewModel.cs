using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class StraightElementViewModel : ElementViewModelBase, ITrackElementViewModel {
    public StraightElementViewModel() {
        Image = ImageSource.FromFile("straight.png");
        Name = "Straight Track";
    }
}
