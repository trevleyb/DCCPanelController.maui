namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class StraightTrackViewModel : TrackViewModelBase {
    
    public StraightTrackViewModel() {
        Image = ImageSource.FromFile("straight.png");
        Name = "Straight Track";
    }
}
