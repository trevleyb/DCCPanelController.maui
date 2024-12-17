using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLeftTurnout(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackTurnoutBase(parent, styleTypeEnum), ITrackTurnout, ITrackSymbol, ITrackPiece {
    public TrackLeftTurnout() : this(null) { }
    protected override void Setup() {
        Name = "Turnout(L)";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "TurnoutL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "TurnoutL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Straight, "TurnoutL2", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Diverging, "TurnoutL3", (0, 0), (90, 90), (180, 180), (270, 270));
    }

    protected override void ThrowTurnout(Turnout turnout, TurnoutStateEnum state) {
        Console.WriteLine($"Turnout '{turnout.Name}' is {(state == TurnoutStateEnum.Closed ? "CLOSED" : state == TurnoutStateEnum.Thrown ? "THROWN" : "UNKNOWN")}");
    }

    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackLeftTurnout>(parent);
    }
}