using System.Reflection.Metadata.Ecma335;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class TileFactory {
    public static ITile? CreateTile(Entity entity, double gridSize) {
        ITile? tile = entity switch {
            ButtonEntity               => new ButtonTile(entity, gridSize),
            CircleEntity               => null,
            CircleLabelEntity          => null,
            CompassEntity              => null,
            CornerEntity               => new CornerTile(entity, gridSize),
            CornerContinuationEntity   => new CornerContinueTile(entity, gridSize),
            CrossingEntity             => null,
            ImageEntity                => null,
            LeftTurnoutEntity          => null,
            LineEntity                 => null,
            PointsEntity               => null,
            RectangleEntity            => null,
            RightTurnoutEntity         => null,
            StraightContinuationEntity => new StraightContinuationTile(entity, gridSize),
            StraightEntity             => new StraightTile(entity, gridSize),
            TerminatorEntity           => new TerminatorTile(entity, gridSize),
            TextEntity                 => null,
            _                          => null
        };
        if (tile is null) {
            Console.WriteLine($"Unknown entity type: {entity.GetType()}");
            return null;
        }
        return tile;
    }
}