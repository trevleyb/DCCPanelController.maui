namespace DCCPanelController.Models.ViewModel.Helpers;

public class TileRenderException : Exception {
    public TileRenderException(Type tileType, Type entityType) : base($"Unable to create a Tile of type '{tileType.Name}' with entity type '{entityType.Name}'.") { }
}