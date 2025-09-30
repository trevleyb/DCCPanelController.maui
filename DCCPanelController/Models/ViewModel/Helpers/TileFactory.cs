using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class TileFactory {
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
                var entityType = constructor.GetParameters()[0].ParameterType;
                mappings.TryAdd(entityType, tileType);
            }
        }
        return mappings;
    });

    public static ITile? CreateTile(Entity entity, double gridSize, bool designMode) {
        var entityType = entity.GetType();
        if (EntityTileMappings.Value.TryGetValue(entityType, out var tileType)) {
            var tile = (ITile?)Activator.CreateInstance(tileType, entity, gridSize);
            if (tile is { }) {
                tile.IsDesignMode = designMode;
                return tile;
            } 
        }
        LogHelper.CreateLogger("TileFactory").LogDebug("No tile found for entity type {entityTypeName}", entityType.Name);
        return null;
    }
}