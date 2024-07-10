namespace DCCPanelController.Symbols.TrackViewModels;

public partial class CrossTrackViewModel : TrackViewModelBase {
    public CrossTrackViewModel() {
        Image = ImageSource.FromFile("crossing.png");
        Name = "Crossing";
    }
}