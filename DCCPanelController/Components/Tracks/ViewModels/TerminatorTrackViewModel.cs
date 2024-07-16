namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class TerminatorTrackViewModel : Base.TrackViewModelBase {
    public TerminatorTrackViewModel() {
        Image = ImageSource.FromFile("terminate.png");
        Name = "Track Terminator";
    }
}