using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackImage(Panel? parent = null) : TrackPieceBase(parent), ITrackSymbol, ITrackPiece {
    public TrackImage() : this(null) { }
    protected override void Setup() {
        Layer = 0;
        Name = "ImageStyle";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
    }
    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackImage>(parent);
    }
}