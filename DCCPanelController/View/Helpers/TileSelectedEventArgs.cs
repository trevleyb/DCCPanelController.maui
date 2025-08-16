using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.View.Helpers;

// Event records
public class TileTappedEventArgs(ITile tile, int tapCount) : TileSelectedEventArgs([tile], tapCount);

public class TileSelectedEventArgs(HashSet<ITile> tiles, int tapCount) : EventArgs {
    private int TapCount { get; init; } = tapCount;
    public HashSet<ITile> Tiles { get; init; } = tiles;
    public ITile? Tile => Tiles.FirstOrDefault();
    public bool IsSingleTap => TapCount == 1;
    public bool IsDoubleTap => TapCount == 2;
    public bool IsLongTap   => TapCount == -1;
}