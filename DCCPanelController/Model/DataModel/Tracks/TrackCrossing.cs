namespace DCCPanelController.Model.DataModel.Tracks;
public partial class TrackCrossing : Track {
    public override string Name => "Crossing Track";
    public TrackCrossing() {}
    public TrackCrossing(TrackCrossing track) : base(track) {}
}