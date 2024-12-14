using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks;

public partial class TrackRightTurnout(Panel? parent = null, TrackStyleType styleType = TrackStyleType.Mainline) : TrackTurnoutBase(parent, styleType), ITrackTurnout, ITrackSymbol, ITrackPiece {
    public TrackRightTurnout() : this(null) { }
    protected override void Setup() {
        SetTrackSymbol("TurnoutR1");
        Name = "Right Turnout";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "TurnoutR1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Straight, "TurnoutR2", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Diverging, "TurnoutR3", (0, 0), (90, 90), (180, 180), (270, 270));
    }
    
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }

    protected override void ThrowTurnout(Turnout turnout, TurnoutStateEnum state) {
        Console.WriteLine($"Turnout '{turnout.Name}' is {(state == TurnoutStateEnum.Closed ? "CLOSED" : state == TurnoutStateEnum.Thrown ? "THROWN" : "UNKNOWN")}");
    }

}