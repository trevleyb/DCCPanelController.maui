namespace DCCPanelController.Model.DataModel.Tracks;
public partial class TrackCompass : Track {
    public override string Name => "Compass";

    public TrackCompass() {}
    public TrackCompass(TrackCompass track) : base(track) {}
}