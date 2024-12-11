using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackTerminator(Panel? parent = null) : TrackPieceBase(parent), ITrackSymbol, ITrackPiece {
    public TrackTerminator() : this(null) { }
    protected override void Setup() {
        SetTrackSymbol("Terminator1");
        Name = "Track Terminator";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Terminator2", (45, 90), (135, 180), (225, 270), (315, 0));
    }
    
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }

}