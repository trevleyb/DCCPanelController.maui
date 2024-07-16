namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class CrossTrackViewModel : Base.TrackViewModelBase {
    public CrossTrackViewModel() {
        Image = ImageSource.FromFile("crossing.png");
        Name = "Crossing";
    }
}