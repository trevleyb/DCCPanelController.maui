using System.Collections.Concurrent;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.PathFinder;

public class TrackPathTracer {
    public class PathSegment {
        public TrackEntity Track { get; set; }
        public TrackTile? Tile { get; set; }
        public int EntryDirection { get; set; }
        public int ExitDirection { get; set; }
        public bool IsTerminal { get; set; }
        
        public PathSegment(TrackEntity track, TrackTile? tile, int entryDirection, int exitDirection, bool isTerminal = false) {
            Track = track;
            Tile = tile;
            EntryDirection = entryDirection;
            ExitDirection = exitDirection;
            IsTerminal = isTerminal;
        }
    }

    public class TracedPath {
        public List<PathSegment> Segments { get; set; } = new();
        public bool IsComplete { get; set; }
        public string StopReason { get; set; } = string.Empty;
    }

    private const int PathAnimationDelayMs = 500;
    private const int PathDisplayDurationMs = 3000;
    
    // Dictionary to look up tiles by their track entities (optional for highlighting)
    private readonly Dictionary<TrackEntity, TrackTile> _trackToTileLookup = new();

    public void RegisterTile(TrackEntity track, TrackTile tile) {
        _trackToTileLookup[track] = tile;
    }

    public void UnregisterTile(TrackEntity track) {
        _trackToTileLookup.Remove(track);
    }

    public void ClearTileRegistry() {
        _trackToTileLookup.Clear();
    }

    public async Task<List<TracedPath>> TracePathsFromTrackAsync(
        TrackEntity startTrack, 
        CancellationToken cancellationToken = default) {
        
        var paths = new List<TracedPath>();
        var visited = new HashSet<TrackEntity>();
        
        // Get the connections directly from the entity
        var startConnections = GetTrackConnections(startTrack);
        if (startConnections == null) return paths;
        
        // Find all valid starting directions (non-None connections)
        var startDirections = GetValidConnections(startConnections, startTrack.Rotation);
        
        foreach (var direction in startDirections) {
            if (cancellationToken.IsCancellationRequested) break;
            
            var path = await TracePathInDirectionAsync(
                startTrack, 
                direction, 
                visited, 
                cancellationToken);
            
            if (path.Segments.Count > 0) {
                paths.Add(path);
            }
        }
        
        return paths;
    }

    public async Task AnimatePathsAsync(
        List<TracedPath> paths, 
        CancellationToken cancellationToken = default) {
        
        try {
            // Clear any existing path highlights
            await ClearAllPathHighlights();
            
            foreach (var path in paths) {
                if (cancellationToken.IsCancellationRequested) break;
                
                await AnimateSinglePathAsync(path, cancellationToken);
            }
            
            // Keep paths visible for a while
            await Task.Delay(PathDisplayDurationMs, cancellationToken);
            
            // Clear highlights
            await ClearAllPathHighlights();
        }
        catch (OperationCanceledException) {
            // Clean up on cancellation
            await ClearAllPathHighlights();
        }
    }

    private async Task<TracedPath> TracePathInDirectionAsync(
        TrackEntity startTrack, 
        int startDirection, 
        HashSet<TrackEntity> globalVisited,
        CancellationToken cancellationToken) {
        
        var path = new TracedPath();
        var localVisited = new HashSet<TrackEntity>();
        var currentTrack = startTrack;
        var currentDirection = startDirection;
        
        while (currentTrack != null && !cancellationToken.IsCancellationRequested) {
            // Check if we've already visited this track in this path
            if (localVisited.Contains(currentTrack)) {
                path.StopReason = "Circular path detected";
                break;
            }
            
            localVisited.Add(currentTrack);
            
            // Get connections for current track from the entity
            var connections = GetTrackConnections(currentTrack);
            if (connections == null) {
                path.StopReason = "No connection information available";
                break;
            }
            
            // Find the exit direction
            var exitInfo = FindExitDirection(currentTrack, connections, currentDirection);
            if (!exitInfo.HasValue) {
                // Add terminal segment
                var tile = _trackToTileLookup.GetValueOrDefault(currentTrack);
                path.Segments.Add(new PathSegment(currentTrack, tile, currentDirection, -1, true));
                path.StopReason = "No valid exit";
                path.IsComplete = true;
                break;
            }
            
            var (exitDirection, stopReason) = exitInfo.Value;
            
            // Add segment to path
            var currentTile = _trackToTileLookup.GetValueOrDefault(currentTrack);
            path.Segments.Add(new PathSegment(currentTrack, currentTile, currentDirection, exitDirection));
            
            if (!string.IsNullOrEmpty(stopReason)) {
                path.StopReason = stopReason;
                path.IsComplete = stopReason == "Path terminated";
                break;
            }
            
            // Find next track
            var nextTrackInfo = FindNextTrack(currentTrack, exitDirection);
            if (nextTrackInfo == null) {
                path.StopReason = "No connecting track found";
                path.IsComplete = true;
                break;
            }
            
            var (nextTrack, entryDirection) = nextTrackInfo.Value;
            
            currentTrack = nextTrack;
            currentDirection = entryDirection;
        }
        
        return path;
    }

    private EntityConnections? GetTrackConnections(TrackEntity track) {
        // Get the connections directly from the entity
        return track.Connections;
    }

    private List<int> GetValidConnections(EntityConnections connections, int rotation) {
        // Use the entity's method to get valid directions at current rotation
        return connections.GetValidDirections(rotation);
    }

    private (int exitDirection, string stopReason)? FindExitDirection(
        TrackEntity track, 
        EntityConnections connections, 
        int entryDirection) {
        
        // Get the current connections for this track's rotation
        var currentConnections = connections.GetConnections(track.Rotation);
        
        // Get the opposite direction (where we came from)
        var oppositeDirection = (entryDirection + 4) % EntityConnections.MaxDirections;
        
        // Handle turnouts specially
        if (track is TurnoutEntity turnout) {
            return HandleTurnoutExit(turnout, currentConnections, entryDirection, oppositeDirection);
        }
        
        // For straight track, find the exit direction
        for (int i = 0; i < EntityConnections.MaxDirections; i++) {
            if (i == oppositeDirection) continue; // Don't go back where we came from
            
            var connectionType = currentConnections[i];
            switch (connectionType) {
                case ConnectionType.Straight:
                case ConnectionType.Closed:
                case ConnectionType.Diverging:
                    return (i, string.Empty);
                    
                case ConnectionType.Terminator:
                case ConnectionType.Connector:
                    return (i, "Path terminated");
            }
        }
        
        return null; // No valid exit found
    }

    private (int exitDirection, string stopReason)? HandleTurnoutExit(
        TurnoutEntity turnout, 
        ConnectionType[] connections, 
        int entryDirection, 
        int oppositeDirection) {
        
        switch (turnout.State) {
            case TurnoutStateEnum.Unknown:
                return (entryDirection, "Turnout state unknown");
                
            case TurnoutStateEnum.Closed:
                // Find the Closed connection
                for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                    if (i == oppositeDirection) continue;
                    if (connections[i] == ConnectionType.Closed) {
                        return (i, string.Empty);
                    }
                }
                break;
                
            case TurnoutStateEnum.Thrown:
                // Find the Diverging connection
                for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                    if (i == oppositeDirection) continue;
                    if (connections[i] == ConnectionType.Diverging) {
                        return (i, string.Empty);
                    }
                }
                break;
        }
        
        return null;
    }

    private (TrackEntity track, int entryDirection)? FindNextTrack(TrackEntity currentTrack, int exitDirection) {
        if (currentTrack.Parent?.GetEntityAtPosition(
            currentTrack.Col + GetDirectionOffset(exitDirection).dx,
            currentTrack.Row + GetDirectionOffset(exitDirection).dy) is TrackEntity nextTrack) {
            
            // Calculate the entry direction for the next track (opposite of our exit)
            var entryDirection = (exitDirection + 4) % EntityConnections.MaxDirections;
            return (nextTrack, entryDirection);
        }
        
        return null;
    }

    private (int dx, int dy) GetDirectionOffset(int direction) {
        return direction switch {
            0 => (0, -1),  // N
            1 => (1, -1),  // NE
            2 => (1, 0),   // E
            3 => (1, 1),   // SE
            4 => (0, 1),   // S
            5 => (-1, 1),  // SW
            6 => (-1, 0),  // W
            7 => (-1, -1), // NW
            _ => (0, 0)
        };
    }

    private async Task AnimateSinglePathAsync(TracedPath path, CancellationToken cancellationToken) {
        foreach (var segment in path.Segments) {
            if (cancellationToken.IsCancellationRequested) break;
            
            // Set the IsPath property on the tile if available
            if (segment.Tile != null) {
                await MainThread.InvokeOnMainThreadAsync(() => {
                    segment.Tile.IsPath = true;
                });
            }
            
            await Task.Delay(PathAnimationDelayMs, cancellationToken);
        }
    }

    private async Task ClearAllPathHighlights() {
        await MainThread.InvokeOnMainThreadAsync(() => {
            // Clear all IsPath properties from registered tiles
            foreach (var tile in _trackToTileLookup.Values) {
                tile.IsPath = false;
            }
        });
    }

    // Utility method to trace paths without animation (useful for route calculation)
    public List<TracedPath> TracePathsFromTrackSync(TrackEntity startTrack) {
        var paths = new List<TracedPath>();
        var visited = new HashSet<TrackEntity>();
        
        // Get the connections directly from the entity
        var startConnections = GetTrackConnections(startTrack);
        if (startConnections == null) return paths;
        
        // Find all valid starting directions
        var startDirections = GetValidConnections(startConnections, startTrack.Rotation);
        
        foreach (var direction in startDirections) {
            var path = TracePathInDirectionSync(startTrack, direction, visited);
            if (path.Segments.Count > 0) {
                paths.Add(path);
            }
        }
        
        return paths;
    }

    private TracedPath TracePathInDirectionSync(
        TrackEntity startTrack, 
        int startDirection, 
        HashSet<TrackEntity> globalVisited) {
        
        var path = new TracedPath();
        var localVisited = new HashSet<TrackEntity>();
        var currentTrack = startTrack;
        var currentDirection = startDirection;
        
        while (currentTrack != null) {
            if (localVisited.Contains(currentTrack)) {
                path.StopReason = "Circular path detected";
                break;
            }
            
            localVisited.Add(currentTrack);
            
            var connections = GetTrackConnections(currentTrack);
            if (connections == null) {
                path.StopReason = "No connection information available";
                break;
            }
            
            var exitInfo = FindExitDirection(currentTrack, connections, currentDirection);
            if (!exitInfo.HasValue) {
                var tile = _trackToTileLookup.GetValueOrDefault(currentTrack);
                path.Segments.Add(new PathSegment(currentTrack, tile, currentDirection, -1, true));
                path.StopReason = "No valid exit";
                path.IsComplete = true;
                break;
            }
            
            var (exitDirection, stopReason) = exitInfo.Value;
            var currentTile = _trackToTileLookup.GetValueOrDefault(currentTrack);
            path.Segments.Add(new PathSegment(currentTrack, currentTile, currentDirection, exitDirection));
            
            if (!string.IsNullOrEmpty(stopReason)) {
                path.StopReason = stopReason;
                path.IsComplete = stopReason == "Path terminated";
                break;
            }
            
            var nextTrackInfo = FindNextTrack(currentTrack, exitDirection);
            if (nextTrackInfo == null) {
                path.StopReason = "No connecting track found";
                path.IsComplete = true;
                break;
            }
            
            var (nextTrack, entryDirection) = nextTrackInfo.Value;
            currentTrack = nextTrack;
            currentDirection = entryDirection;
        }
        
        return path;
    }
}

// Usage service class (unchanged interface)
public class PathTracingService {
    private TrackPathTracer _tracer = new();
    private CancellationTokenSource? _currentTracingCts;

    public void RegisterTile(TrackEntity track, TrackTile tile) {
        _tracer.RegisterTile(track, tile);
    }

    public void UnregisterTile(TrackEntity track) {
        _tracer.UnregisterTile(track);
    }

    public void ClearTileRegistry() {
        _tracer.ClearTileRegistry();
    }

    public async Task StartPathTracing(TrackEntity startTrack) {
        // Cancel any existing tracing
        await StopPathTracing();
        
        _currentTracingCts = new CancellationTokenSource();
        
        try {
            var paths = await _tracer.TracePathsFromTrackAsync(startTrack, _currentTracingCts.Token);
            await _tracer.AnimatePathsAsync(paths, _currentTracingCts.Token);
        }
        catch (OperationCanceledException) {
            // Tracing was cancelled
        }
        finally {
            _currentTracingCts?.Dispose();
            _currentTracingCts = null;
        }
    }

    public async Task StopPathTracing() {
        _currentTracingCts?.Cancel();
        _currentTracingCts?.Dispose();
        _currentTracingCts = null;
        
        // Allow some time for cleanup
        await Task.Delay(100);
    }

    // Synchronous path tracing for route calculation
    public List<TrackPathTracer.TracedPath> GetPathsFromTrack(TrackEntity startTrack) {
        return _tracer.TracePathsFromTrackSync(startTrack);
    }
}