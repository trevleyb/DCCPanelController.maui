using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackImage : TrackPieceBase, ITrackSymbol, ITrackPiece {
    protected override void Setup() {
        Layer = 0;
        Name = "Image";
        SetTrackSymbol("Image");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
    }
    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}