namespace DCCPanelController.Symbols.TrackViewModels;

public partial class StraightTrackViewModel : TrackViewModelBase {
    
    public StraightTrackViewModel() {
        Image = ImageSource.FromFile("straight.png");
        Name = "Straight Track";
    }
}
