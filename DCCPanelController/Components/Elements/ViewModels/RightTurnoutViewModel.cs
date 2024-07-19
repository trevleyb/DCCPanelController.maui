using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class RightTurnoutViewModel : ElementViewModelBase, ITrackElementViewModel {
    public RightTurnoutViewModel() {
        Image = ImageSource.FromFile("turnoutright.png");
        Name = "Turnout (Right)";
    }
}