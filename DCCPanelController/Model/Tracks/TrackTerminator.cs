using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackTerminator(Panel? parent = null, TrackStyleType styleType = TrackStyleType.Mainline) : TrackPieceBase(parent, styleType), ITrackSymbol, ITrackPiece {
    public TrackTerminator() : this(null) { }
    protected override void Setup() {
        Name = "Terminator";
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Terminator2", (45, 90), (135, 180), (225, 270), (315, 0));
    }
    public ITrackPiece Clone(Panel parent) {
        var track = (TrackTerminator)MemberwiseClone();
        track.Id = Guid.NewGuid();
        track.Parent = parent;
        return track;
    }
}