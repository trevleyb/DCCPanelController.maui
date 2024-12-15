using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLeftTurnout(Panel? parent = null, TrackStyleType styleType = TrackStyleType.Mainline) : TrackTurnoutBase(parent, styleType), ITrackTurnout, ITrackSymbol, ITrackPiece {
    public TrackLeftTurnout() : this(null) { }
    protected override void Setup() {
        Name = "Turnout(L)";
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "TurnoutL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "TurnoutL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Straight, "TurnoutL2", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Diverging, "TurnoutL3", (0, 0), (90, 90), (180, 180), (270, 270));
    }

    protected override void ThrowTurnout(Turnout turnout, TurnoutStateEnum state) {
        Console.WriteLine($"Turnout '{turnout.Name}' is {(state == TurnoutStateEnum.Closed ? "CLOSED" : state == TurnoutStateEnum.Thrown ? "THROWN" : "UNKNOWN")}");
    }
}