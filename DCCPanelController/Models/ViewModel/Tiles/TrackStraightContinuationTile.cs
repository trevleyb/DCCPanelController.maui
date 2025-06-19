using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackStraightContinuationTile : TrackTile {
    public TrackStraightContinuationTile(StraightContinuationEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(StraightContinuationEntity.ContinuationStyle));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return ((StraightContinuationEntity)Entity).ContinuationStyle switch {
            TrackTerminatorEnum.Arrow => CreateTrackTile("StraightContinuationArrow", Entity.Rotation),
            TrackTerminatorEnum.Lines => CreateTrackTile("StraightContinuationLines", Entity.Rotation),
            _                         => CreateTrackTile("StraightContinuationArrow", Entity.Rotation)
        };
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("StraightContinuationArrow", Entity.Rotation, SymbolScaleFactor);
    }
}