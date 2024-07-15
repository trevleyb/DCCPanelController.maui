namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class CrossTrackViewModel : TrackViewModelBase {
    public CrossTrackViewModel() {
        Image = ImageSource.FromFile("crossing.png");
        Name = "Crossing";
    }
}