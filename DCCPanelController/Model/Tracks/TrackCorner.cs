using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackCorner(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackSymbol, ITrack {

    public string Name => "Corner Track";
    public TrackCorner() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackCorner>(parent);
    }

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "CornerR", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "CornerL", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "CornerR", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "CornerL", (45, 270), (135, 0), (225, 90), (315, 180));
    }
}