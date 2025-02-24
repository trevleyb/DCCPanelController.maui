using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackStraight(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackSymbol, ITrack {
    public TrackStraight() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackStraight>(parent);
    }

    [ObservableProperty]
    private string _name = "Straight Track";

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Straight1", (0, 0), (90, 90), (180, 0), (270, 90));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Straight1", (0, 0), (90, 90), (180, 0), (270, 90));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Straight2", (45, 0), (135, 90), (225, 0), (315, 90));
    }
}