namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class RightTrackViewModel : TrackViewModelBase{
    public RightTrackViewModel() {
        Image = ImageSource.FromFile("angleright.png");
        Name = "Right Track";
    }
}