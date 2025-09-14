using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackCornerContinueTile : TrackTile {
    public TrackCornerContinueTile(CornerContinuationEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) => VisualProperties.Add(nameof(CornerContinuationEntity.ContinuationStyle));

    protected override Microsoft.Maui.Controls.View? CreateTile() => ((CornerContinuationEntity)Entity).ContinuationStyle switch {
        TrackTerminatorEnum.Arrow => CreateTrackTile("CornerContinuationArrow", Entity.Rotation),
        TrackTerminatorEnum.Lines => CreateTrackTile("CornerContinuationLines", Entity.Rotation),
        _                         => CreateTrackTile("CornerContinuationArrow", Entity.Rotation),
    };

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTrackTile("CornerContinuationArrow", Entity.Rotation, SymbolScaleFactor);
}