using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class CornerContinueTile(Entity entity, double gridSize) : TrackTile(entity, gridSize) {
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return ((StraightContinuationEntity)Entity).ContinuationStyle switch {
            TrackTerminatorEnum.Arrow => CreateTrackTile("CornerContinuationArrow", Entity.Rotation),
            TrackTerminatorEnum.Lines => CreateTrackTile("CornerContinuationLines", Entity.Rotation),
            _                         => CreateTrackTile("CornerContinuationArrow", Entity.Rotation),
        };
    }
}