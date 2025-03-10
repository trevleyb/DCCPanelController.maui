using System.Reflection.Metadata.Ecma335;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class TileFactory {
    public static ITile? CreateTile(Entity entity, double gridSize) {
        ITile? tile = entity switch {
            ButtonEntity    => new ButtonTile(entity, gridSize),
            StraightEntity  => new StraightTile(entity, gridSize),
            _               => null
        };
        if (tile is null) {
            Console.WriteLine($"Unknown entity type: {entity.GetType()}");
            return null;
        }
        tile.CreateTile();
        return tile;
    }
}