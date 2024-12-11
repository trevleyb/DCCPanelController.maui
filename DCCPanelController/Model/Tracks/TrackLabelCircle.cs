using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLabelCircle(Panel? parent = null) : TrackLabelBase(parent), ITrackSymbol, ITrackPiece {
    public TrackLabelCircle() : this(null) { }
    protected override void Setup() {
        Layer = 2;
        Name="Label";
        SetTrackSymbol("Label");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Label", (0, 0), (90, 0), (180, 0), (270, 0));
    }

    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }


}