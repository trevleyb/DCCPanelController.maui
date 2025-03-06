namespace DCCPanelController.Model.DataModel.Tracks;
public partial class TrackCorner : Track {
    public override string Name => "Corner Track";
    public TrackCorner() {}
    public TrackCorner(TrackCorner track) : base(track) {}
}