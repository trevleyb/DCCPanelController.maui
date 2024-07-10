namespace DCCPanelController.Symbols.TrackViewModels;

public partial class TerminatorTrackViewModel : TrackViewModelBase {
    public TerminatorTrackViewModel() {
        Image = ImageSource.FromFile("terminate.png");
        Name = "Track Terminator";
    }
}