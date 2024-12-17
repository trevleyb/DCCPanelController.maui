using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackCrossing(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackSymbol, ITrackPiece {
    public TrackCrossing() : this(null) { }
    protected override void Setup() {
        Name = "Crossing";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Cross1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Cross2", (45, 0), (135, 90), (225, 0), (315, 90));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Cross1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Cross2", (45, 0), (135, 90), (225, 0), (315, 90));
    }
    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackCrossing>(parent);    
    }
}