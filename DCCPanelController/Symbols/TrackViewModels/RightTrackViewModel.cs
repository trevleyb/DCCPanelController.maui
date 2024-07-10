namespace DCCPanelController.Symbols.TrackViewModels;

public partial class RightTrackViewModel : TrackViewModelBase{
    public RightTrackViewModel() {
        Image = ImageSource.FromFile("angleright.png");
        Name = "Right Track";
    }
}