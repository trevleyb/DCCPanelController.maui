using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackTerminator : TrackPieceBase, ITrackSymbol, ITrackPiece {
    protected override void Setup() {
        SetTrackSymbol("Terminator1");
        Name = "Track Terminator";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Terminator1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Terminator2", (45, 90), (135, 180), (225, 270), (315, 0));
    }
    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}