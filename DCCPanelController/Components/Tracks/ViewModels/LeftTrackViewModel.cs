namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class LeftTrackViewModel : TrackViewModelBase {
    public LeftTrackViewModel() {
        Image = ImageSource.FromFile("angleleft.png");
        Name = "Left Track";
    }
}