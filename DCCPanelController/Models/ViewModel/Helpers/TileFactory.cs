using System.Reflection.Metadata.Ecma335;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class TileFactory {
    public static ITile? CreateTile(Entity entity, double gridSize) {
        ITile? tile = entity switch {
            ButtonEntity               => new ButtonTile(entity, gridSize),
            CompassEntity              => new CompassTile(entity, gridSize),
            CornerEntity               => new CornerTile(entity, gridSize),
            CornerContinuationEntity   => new CornerContinueTile(entity, gridSize),
            CrossingEntity             => new CrossingTile(entity, gridSize),
            LeftTurnoutEntity          => null,
            RightTurnoutEntity         => null,
            StraightContinuationEntity => new StraightContinuationTile(entity, gridSize),
            StraightEntity             => new StraightTile(entity, gridSize),
            TerminatorEntity           => new TerminatorTile(entity, gridSize),
            PointsEntity               => null,
            ImageEntity                => null,
            TextEntity                 => null,
            RectangleEntity            => null,
            LineEntity                 => null,
            CircleEntity               => null,
            CircleLabelEntity          => null,
            _                          => null
        };
        if (tile is null) {
            Console.WriteLine($"Unknown entity type: {entity.GetType()}");
            return null;
        }
        return tile;
    }
}