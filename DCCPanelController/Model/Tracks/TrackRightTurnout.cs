using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackRightTurnout(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackTurnout(parent, styleTypeEnum), ITrackTurnout, ITrackSymbol, ITrack {

    public string Name => "Right Turnout";

    public TrackRightTurnout() : this(null) { }

    public ITrack Clone(Panel parent) {
        var clone = Clone<TrackRightTurnout>(parent);
        clone.Address = "";
        clone.TurnoutID = parent.NextTurnoutID();
        return clone;
    }

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "TurnoutR1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "TurnoutR1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Straight, "TurnoutR2", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Diverging, "TurnoutR3", (0, 0), (90, 90), (180, 180), (270, 270));
    }

    protected override void ThrowTurnout(Turnout turnout, TurnoutStateEnum state) {
        Console.WriteLine($"Turnout '{turnout.Name}' is {(state == TurnoutStateEnum.Closed ? "CLOSED" : state == TurnoutStateEnum.Thrown ? "THROWN" : "UNKNOWN")}");
    }
}