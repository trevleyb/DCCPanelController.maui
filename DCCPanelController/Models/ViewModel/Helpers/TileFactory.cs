using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using ExCSS;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class TileFactory {
    public static ITile? CreateTile(Entity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) {
        using (new CodeTimer($"CreateTile [entity.Type={entity.Type}]")) {
            var entityType = entity.GetType();
            if (EntityTileMappings.Value.TryGetValue(entityType, out var tileType)) {
                return (ITile?)Activator.CreateInstance(tileType, entity, gridSize, displayMode);
            }
            Console.WriteLine($"No tile found for entity type {entityType.Name}");
            return null;
        }
    }

    private static readonly Lazy<Dictionary<Type, Type>> EntityTileMappings = new(() => {
        // Get all tile types implementing ITile
        var tileTypes = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(type => typeof(ITile).IsAssignableFrom(type) && !type.IsAbstract)
                                .ToList();

        var mappings = new Dictionary<Type, Type>();
        foreach (var tileType in tileTypes) {
            // Find a constructor with parameters where the first parameter is an Entity type
            var constructor = tileType.GetConstructors()
                                      .FirstOrDefault(ctor => {
                                           var parameters = ctor.GetParameters();
                                           return parameters.Length > 0 &&
                                                  typeof(Entity).IsAssignableFrom(parameters[0].ParameterType);
                                       });

            if (constructor != null) {
                // Use the first parameter of the constructor to determine the Entity type
                var entityType = constructor.GetParameters()[0].ParameterType;

                // Ensure we map each Entity type to its corresponding Tile type
                if (!mappings.ContainsKey(entityType)) {
                    mappings[entityType] = tileType;
                }
            }
        }
        return mappings;
    });

    // Cache mapping of entity types to their corresponding tile types for performance
    private static readonly Lazy<Dictionary<Type, Type>> EntityTileMappingsOld = new(() => {
        // Get all tile types implementing ITile
        var tileTypes = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(type => typeof(ITile).IsAssignableFrom(type) && !type.IsAbstract)
                                .ToList();

        // Map each entity type to its corresponding tile type
        var mappings = new Dictionary<Type, Type>();
        foreach (var tileType in tileTypes) {
            // Check if the tile type has an appropriate constructor
            var constructor = tileType.GetConstructor(new[] { typeof(Entity), typeof(double), typeof(TileDisplayMode) });
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