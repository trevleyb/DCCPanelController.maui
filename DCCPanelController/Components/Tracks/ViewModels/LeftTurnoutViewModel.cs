namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class LeftTurnoutViewModel : TrackViewModelBase{
    public LeftTurnoutViewModel() {
        Image = ImageSource.FromFile("turnoutleft.png");
        Name = "Turnout (Left)";
    }
}