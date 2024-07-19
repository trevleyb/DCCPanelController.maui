using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class LeftTurnoutViewModel : ElementViewModelBase, ITrackElementViewModel{
    public LeftTurnoutViewModel() {
        Image = ImageSource.FromFile("turnoutleft.png");
        Name = "Turnout (Left)";
    }
}