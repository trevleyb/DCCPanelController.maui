namespace DCCPanelController.Symbols.TrackViewModels;

public partial class LeftTrackViewModel : TrackViewModelBase {
    public LeftTrackViewModel() {
        Image = ImageSource.FromFile("angleleft.png");
        Name = "Left Track";
    }
}