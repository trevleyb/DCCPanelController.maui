using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackRightTurnout : TrackTurnout {
    public override string Name => "Right Turnout";
    public TrackRightTurnout() {}
    public TrackRightTurnout(TrackRightTurnout track) : base(track) {}
}