using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class StraightContinuationTile(Entity entity, double gridSize) : TrackTile(entity, gridSize) {
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return ((StraightContinuationEntity)Entity).ContinuationStyle switch {
            TrackTerminatorEnum.Arrow => CreateTrackTile("StraightContinuationArrow", Entity.Rotation),
            TrackTerminatorEnum.Lines => CreateTrackTile("StraightContinuationLines", Entity.Rotation),
            _                         => CreateTrackTile("StraightContinuationArrow", Entity.Rotation),
        };
    }
}