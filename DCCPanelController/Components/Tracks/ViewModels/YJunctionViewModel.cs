namespace DCCPanelController.Components.Tracks.ViewModels;

public partial class YJunctionViewModel : TrackViewModelBase {
    public YJunctionViewModel() {
        Image = ImageSource.FromFile("yjunction.png");
        Name = "Wye Junction";
    }
}