using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackStraight : TrackPieceBase, ITrackSymbol, ITrackPiece {
    protected override void Setup() {
        SetTrackSymbol("Straight1");
        Name = "Straight Track";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Straight1", (0, -90), (90, 0), (180, 90), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Straight2", (45, 0), (135, 90), (225, 0), (315, -90));
    }
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }

}