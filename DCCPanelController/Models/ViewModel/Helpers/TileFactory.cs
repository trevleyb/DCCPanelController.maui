using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class TileFactory {
    
    public static ITile? CreateTile(Entity entity, double gridSize) {
        var entityType = entity.GetType();
        if (EntityTileMappings.Value.TryGetValue(entityType, out var tileType)) {
            return (ITile?)Activator.CreateInstance(tileType, entity, gridSize);
        }
        Console.WriteLine($"No tile found for entity type {entityType.Name}");
        return null;
    }

    public static ITile? CreateTileOld(Entity entity, double gridSize) {
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
            CircleEntity               => new CircleTile(entity, gridSize),
            CircleLabelEntity          => null,
            _                          => null
        };
        return tile;
    }
    
    // Cache mapping of entity types to their corresponding tile types for performance
    private static readonly Lazy<Dictionary<Type, Type>> EntityTileMappings = new(() => {
        // Get all tile types implementing ITile
        var tileTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => typeof(ITile).IsAssignableFrom(type) && !type.IsAbstract)
            .ToList();

        // Map each entity type to its corresponding tile type
        var mappings = new Dictionary<Type, Type>();
        foreach (var tileType in tileTypes) {
            // Check if the tile type has an appropriate constructor
            var constructor = tileType.GetConstructor(new[] { typeof(Entity), typeof(double) });
            if (constructor == null) continue;

            // Infer the entity type from the name convention (e.g., ButtonTile -> ButtonEntity)
            var entityTypeName = tileType.Name.Replace("Tile", "Entity");
            var entityType = Assembly.GetExecutingAssembly()
                .GetType($"DCCPanelController.Models.DataModel.Entities.{entityTypeName}");

            if (entityType != null && typeof(Entity).IsAssignableFrom(entityType)) {
                mappings[entityType] = tileType;
            }
        }
        return mappings;
    });
}