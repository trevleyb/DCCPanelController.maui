namespace DCCPanelController.Symbols.TrackViewModels;

public partial class RightTurnoutViewModel : TrackViewModelBase {
    public RightTurnoutViewModel() {
        Image = ImageSource.FromFile("tuirnoutright.png");
        Name = "Turnout (Right)";
    }
}