using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class CornerContinueTile : TrackTile {
    public CornerContinueTile(CornerContinuationEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return ((CornerContinuationEntity)Entity).ContinuationStyle switch {
            TrackTerminatorEnum.Arrow => CreateTrackTile("CornerContinuationArrow", Entity.Rotation),
            TrackTerminatorEnum.Lines => CreateTrackTile("CornerContinuationLines", Entity.Rotation),
            _                         => CreateTrackTile("CornerContinuationArrow", Entity.Rotation),
        };
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("CornerContinuationArrow").AsImage();
    }

}