using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackStraight : Track {
    public override string Name => "Straight Track";
    public TrackStraight() {}
    public TrackStraight(TrackStraight track) : base(track) {}
}