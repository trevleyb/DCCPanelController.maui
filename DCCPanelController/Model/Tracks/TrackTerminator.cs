using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackTerminator(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackSymbol, ITrackPiece {
    public TrackTerminator() : this(null) { }
    protected override void Setup() {
        Name = "Terminator";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Terminator2", (45, 90), (135, 180), (225, 270), (315, 0));
    }
    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackTerminator>(parent);
    }
}