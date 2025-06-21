using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.PathFinder;

public class PathSegment(TrackEntity track, TrackTile? tile, int entryDirection, int exitDirection, bool isTerminal = false) {
    public TrackEntity Track { get; set; } = track;
    public TrackTile? Tile { get; set; } = tile;
    public int EntryDirection { get; set; } = entryDirection;
    public int ExitDirection { get; set; } = exitDirection;
    public bool IsTerminal { get; set; } = isTerminal;
}

public class TracedPath {
    public List<PathSegment> Segments { get; set; } = new();
    public bool IsComplete { get; set; }
    public string StopReason { get; set; } = string.Empty;
}