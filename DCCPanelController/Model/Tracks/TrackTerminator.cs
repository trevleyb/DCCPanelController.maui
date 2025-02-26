using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackTerminator(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackSymbol, ITrack {

    public string Name => "Terminator Track";

    public TrackTerminator() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackTerminator>(parent);
    }

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Terminator2", (45, 90), (135, 180), (225, 270), (315, 0));
    }
}