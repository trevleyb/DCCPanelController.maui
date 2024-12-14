using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackCrossing(Panel? parent = null, TrackStyleType styleType = TrackStyleType.Mainline) : TrackPieceBase(parent, styleType), ITrackSymbol, ITrackPiece {
    public TrackCrossing() : this(null) { }
    protected override void Setup() {
        SetTrackSymbol("Cross1");
        Name = "Crossing";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Cross1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Cross2", (45, 0), (135, 90), (225, 0), (315, 90));
    }
    
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }



}