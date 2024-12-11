using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackImage(Panel? parent = null) : TrackPieceBase(parent), ITrackSymbol, ITrackPiece {
    public TrackImage() : this(null) { }
    protected override void Setup() {
        Layer = 0;
        Name = "DisplayImage";
        SetTrackSymbol("DisplayImage");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "DisplayImage", (0, 0), (90, 0), (180, 0), (270, 0));
    }
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }


}