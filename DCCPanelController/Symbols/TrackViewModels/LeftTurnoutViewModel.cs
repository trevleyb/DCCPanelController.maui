namespace DCCPanelController.Symbols.TrackViewModels;

public partial class LeftTurnoutViewModel : TrackViewModelBase{
    public LeftTurnoutViewModel() {
        Image = ImageSource.FromFile("turnoutleft.png");
        Name = "Turnout (Left)";
    }
}