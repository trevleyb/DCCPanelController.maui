using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Helpers;

public class TileChangedEventArgs : EventArgs {
    public TileChangedEventArgs(ITile tile, TileChangeType changeType = TileChangeType.Modified) {
        Tile = tile;
        ChangeType = changeType;
    }

    public TileChangedEventArgs(ITile tile, string propertyName, object? oldValue, object? newValue) {
        Tile = tile;
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
        ChangeType = TileChangeType.PropertyChanged;
    }

    public ITile Tile { get; }
    public string? PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    public TileChangeType ChangeType { get; }
}

public enum TileChangeType {
    Modified,
    PropertyChanged,
    Dimensions,
    Visual
}