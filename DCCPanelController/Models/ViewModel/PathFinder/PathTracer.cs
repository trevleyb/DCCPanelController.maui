using System.Collections.Concurrent;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
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

    private const int PathAnimationDelayMs = 100;
    private const int PathDisplayDurationMs = 1500;

    // Dictionary to look up tiles by their track entities (optional for highlighting)
    private readonly Dictionary<TrackEntity, TrackTile> _trackToTileLookup = new();

    public List<TrackEntity> RegisteredEntities => _trackToTileLookup.Keys.ToList();
    public List<TrackTile> RegisteredTiles => _trackToTileLookup.Values.ToList();

    public void RegisterTile(TrackEntity track, TrackTile tile) {
        _trackToTileLookup[track] = tile;
    }

    public void UnregisterTile(TrackEntity track) {
        _trackToTileLookup.Remove(track);
    }

    public void ClearTileRegistry() {
        _trackToTileLookup.Clear();
    }

    public async Task<List<TracedPath>> TracePathsFromTrackAsync(TrackEntity startTrack,
                                                                 CancellationToken cancellationToken = default) {
        var paths = new List<TracedPath>();

        // Get the connections directly from the entity
        var startConnections = GetTrackConnections(startTrack);
        if (startConnections == null) {
            Console.WriteLine("No connections found for starting track");
            return paths;
        }

        // Find all valid starting directions (non-None connections)
        var startDirections = GetValidDirections(startConnections, startTrack.Rotation);
        Console.WriteLine($"Starting track at ({startTrack.Col}, {startTrack.Row}) rotation {startTrack.Rotation}°");
        Console.WriteLine($"Base pattern: {startConnections}");
        Console.WriteLine($"Found {startDirections.Count} valid starting directions: [{string.Join(", ", startDirections)}]");

        // Log the connection types for debugging
        var currentConnections = startConnections.GetConnections(startTrack.Rotation);
        for (int i = 0; i < currentConnections.Length; i++) {
            if (currentConnections[i] != ConnectionType.None) {
                Console.WriteLine($"Direction {i}: {currentConnections[i]}");
            }
        }

        foreach (var direction in startDirections) {
            if (cancellationToken.IsCancellationRequested) break;

            Console.WriteLine($"=== Tracing path in direction {direction} ===");

            var path = await TracePathInDirectionAsync(
                startTrack,
                direction,
                cancellationToken);

            if (path.Segments.Count > 0) {
                Console.WriteLine($"Path in direction {direction} found {path.Segments.Count} segments");
                paths.Add(path);
            } else {
                Console.WriteLine($"No segments found for direction {direction}");
            }
        }

        Console.WriteLine($"Total paths found: {paths.Count}");
        return paths;
    }

    public async Task AnimatePathsAsync(List<TracedPath> paths,
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
        } catch (OperationCanceledException) {
            // Clean up on cancellation
            await ClearAllPathHighlights();
        }
    }

    private async Task<TracedPath> TracePathInDirectionAsync(TrackEntity startTrack,
                                                             int startDirection,
                                                             CancellationToken cancellationToken) {
        var path = new TracedPath();
        var visited = new HashSet<TrackEntity>();
        var currentTrack = startTrack;
        var currentDirection = startDirection;

        Console.WriteLine($"Starting path trace from track at ({currentTrack.Col}, {currentTrack.Row}) in direction {currentDirection}");

        while (currentTrack != null && !cancellationToken.IsCancellationRequested) {
            Console.WriteLine($"Processing track at ({currentTrack.Col}, {currentTrack.Row}), direction: {currentDirection}");

            // Check if we've already visited this track in this path
            if (visited.Contains(currentTrack)) {
                Console.WriteLine($"Circular path detected at ({currentTrack.Col}, {currentTrack.Row})");
                path.StopReason = "Circular path detected";
                break;
            }

            visited.Add(currentTrack);

            // Get connections for current track from the entity
            var connections = GetTrackConnections(currentTrack);
            if (connections == null) {
                Console.WriteLine($"No connection information available for track at ({currentTrack.Col}, {currentTrack.Row})");
                path.StopReason = "No connection information available";
                break;
            }

            // Find the exit direction
            var exitInfo = FindExitDirection(currentTrack, connections, currentDirection);
            if (!exitInfo.HasValue) {
                Console.WriteLine($"No valid exit found for track at ({currentTrack.Col}, {currentTrack.Row})");

                // Add terminal segment
                var tile = _trackToTileLookup.GetValueOrDefault(currentTrack);
                path.Segments.Add(new PathSegment(currentTrack, tile, currentDirection, -1, true));
                path.StopReason = "No valid exit";
                path.IsComplete = true;
                break;
            }

            var (exitDirection, stopReason) = exitInfo.Value;
            Console.WriteLine($"Exit direction: {exitDirection}, Stop reason: {stopReason}");

            // Add segment to path
            var currentTile = _trackToTileLookup.GetValueOrDefault(currentTrack);
            path.Segments.Add(new PathSegment(currentTrack, currentTile, currentDirection, exitDirection));

            if (!string.IsNullOrEmpty(stopReason)) {
                Console.WriteLine($"Path stopping: {stopReason}");
                path.StopReason = stopReason;
                path.IsComplete = true;
                break;
            }

            // Find next track and check connection compatibility
            var nextTrackInfo = FindNextTrackAndValidateConnection(currentTrack, exitDirection);
            if (nextTrackInfo == null) {
                Console.WriteLine($"No connecting track found from ({currentTrack.Col}, {currentTrack.Row}) in direction {exitDirection}");
                path.StopReason = "No connecting track found";
                path.IsComplete = true;
                break;
            }

            var (nextTrack, entryDirection, connectionResult) = nextTrackInfo.Value;

            if (!connectionResult.IsValid) {
                Console.WriteLine($"Connection validation failed: {connectionResult.Reason}");

                // If the reason is "Path terminated at terminator", this is a valid end
                if (connectionResult.Reason.Contains("terminated")) {
                    path.StopReason = connectionResult.Reason;
                    path.IsComplete = true;
                } else {
                    path.StopReason = connectionResult.Reason;
                    path.IsComplete = false;
                }
                break;
            }

            Console.WriteLine($"Found next track at ({nextTrack.Col}, {nextTrack.Row}), entry direction: {entryDirection}");

            currentTrack = nextTrack;
            currentDirection = entryDirection;
        }

        Console.WriteLine($"Path trace complete. Segments: {path.Segments.Count}, Reason: {path.StopReason}");
        return path;
    }

    private EntityConnections? GetTrackConnections(TrackEntity track) {
        return track.Connections;
    }

    private List<int> GetValidDirections(EntityConnections connections, int rotation) {
        // Get all directions that have actual connections (not None)
        // For starting directions, we can start from any connection type EXCEPT:
        // - None (no connection)
        // When determining valid starting directions, we should include all connection types
        // because the restriction on T and C is about ENTERING through them, not starting FROM them
        var validDirections = new List<int>();
        var currentConnections = connections.GetConnections(rotation);

        for (int i = 0; i < EntityConnections.MaxDirections; i++) {
            var connectionType = currentConnections[i];

            // Allow starting from any connection type that exists
            // The entry/exit restrictions will be handled in FindExitDirection and ValidateConnection
            if (connectionType != ConnectionType.None) {
                validDirections.Add(i);
            }
        }

        Console.WriteLine($"GetValidDirections: Found {validDirections.Count} valid starting directions");
        for (int i = 0; i < validDirections.Count; i++) {
            var dir = validDirections[i];
            Console.WriteLine($"  Direction {dir}: {currentConnections[dir]}");
        }

        return validDirections;
    }

    private (int exitDirection, string stopReason)? FindExitDirection(TrackEntity track,
                                                                      EntityConnections connections,
                                                                      int entryDirection) {
        // Get the current connections for this track's rotation
        var currentConnections = connections.GetConnections(track.Rotation);

        // Get the entry connection type
        var entryConnectionType = currentConnections[entryDirection];

        Console.WriteLine($"FindExitDirection: Track at ({track.Col}, {track.Row}), entry dir {entryDirection} = {entryConnectionType}");

        // Verify we can actually enter from this direction
        if (entryConnectionType == ConnectionType.None) {
            Console.WriteLine("Cannot enter from this direction - connection type is None");
            return null; // Can't enter from this direction
        }

        // Special case: If we're STARTING from a Terminator or Connector connection point,
        // this means we're beginning our trace from the endpoint of the track.
        // We need to find the paired connection to continue.
        if (entryConnectionType == ConnectionType.Terminator) {
            Console.WriteLine("Starting from Terminator connection point - looking for paired S connection");

            // Find the Straight connection that pairs with this Terminator
            for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                if (i == entryDirection) continue;
                var connectionType = currentConnections[i];
                if (connectionType == ConnectionType.Straight) {
                    Console.WriteLine($"Found paired Straight connection at direction {i}");
                    return (i, string.Empty); // Continue the path through the S connection
                }
            }
            Console.WriteLine("No paired Straight connection found for Terminator");
            return null;
        }

        if (entryConnectionType == ConnectionType.Connector) {
            Console.WriteLine("Starting from Connector connection point - looking for paired S connection");

            // Find the Straight connection that pairs with this Connector
            for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                if (i == entryDirection) continue;
                var connectionType = currentConnections[i];
                if (connectionType == ConnectionType.Straight) {
                    Console.WriteLine($"Found paired Straight connection at direction {i}");
                    return (i, string.Empty); // Continue the path through the S connection
                }
            }
            Console.WriteLine("No paired Straight connection found for Connector");
            return null;
        }

        // For normal entry (not starting from T or C), cannot enter through T or C
        if (entryConnectionType == ConnectionType.Terminator) {
            Console.WriteLine("Cannot enter through Terminator connection point from another track");
            return null;
        }

        if (entryConnectionType == ConnectionType.Connector) {
            Console.WriteLine("Cannot enter through Connector connection point from another track");
            return null;
        }

        // Handle turnouts specially
        if (track is TurnoutEntity turnout) {
            return HandleTurnoutExit(turnout, currentConnections, entryDirection);
        }

        // Look for exit connections - check if we're connecting to a Terminator or Connector endpoint
        for (int i = 0; i < EntityConnections.MaxDirections; i++) {
            if (i == entryDirection) continue; // Don't go back the same way

            var connectionType = currentConnections[i];

            // Special handling for Terminators and Connectors - they are endpoints
            if (connectionType == ConnectionType.Terminator) {
                Console.WriteLine($"Found Terminator at direction {i} - this ends the path");
                return (i, "Path terminated at terminator end");
            }

            if (connectionType == ConnectionType.Connector) {
                Console.WriteLine($"Found Connector at direction {i} - this ends the path");
                return (i, "Path terminated at connector end");
            }

            // Check if this connection type can pair with our entry (normal flow)
            if (CanConnectionsPair(entryConnectionType, connectionType)) {
                Console.WriteLine($"Found exit direction {i} with connection type {connectionType}");
                return (i, string.Empty); // Continue the path
            }
        }

        Console.WriteLine("No valid exit direction found");
        return null; // No valid exit found
    }

    private bool CanConnectionsPair(ConnectionType entryType, ConnectionType exitType) {
        // Based on your rules - these are internal connections within a single track piece
        // This determines if entry and exit points on the SAME track can connect

        return (entryType, exitType) switch {
            // Straight connections - can pair with anything except Terminators and Connectors
            (ConnectionType.Straight, ConnectionType.Straight)  => true,
            (ConnectionType.Straight, ConnectionType.Closed)    => true,
            (ConnectionType.Straight, ConnectionType.Diverging) => true,

            // Closed (X) connections - can pair with Straight and other closed
            (ConnectionType.Closed, ConnectionType.Closed)    => true,
            (ConnectionType.Closed, ConnectionType.Straight)  => true,
            (ConnectionType.Closed, ConnectionType.Diverging) => true,

            // Diverging (D) connections - can pair with Straight and other diverging  
            (ConnectionType.Diverging, ConnectionType.Diverging) => true,
            (ConnectionType.Diverging, ConnectionType.Straight)  => true,
            (ConnectionType.Diverging, ConnectionType.Closed)    => true,

            // IMPORTANT: Terminators and Connectors are ENDPOINTS
            // You can ENTER them from S, but you CANNOT EXIT from them
            // They do NOT pair with anything as an exit point
            (ConnectionType.Terminator, _) => false, // Cannot exit from Terminator
            (ConnectionType.Connector, _)  => false, // Cannot exit from Connector

            // You also cannot ENTER through a Terminator or Connector
            (_, ConnectionType.Terminator) => false, // Cannot enter through Terminator  
            (_, ConnectionType.Connector)  => false, // Cannot enter through Connector

            _ => false
        };
    }

    private (int exitDirection, string stopReason)? HandleTurnoutExit(TurnoutEntity turnout,
                                                                      ConnectionType[] connections,
                                                                      int entryDirection) {
        var entryConnectionType = connections[entryDirection];

        switch (turnout.State) {
        case TurnoutStateEnum.Unknown:
            return (entryDirection, "Turnout state unknown");

        case TurnoutStateEnum.Closed:
            // Find the Closed (X) connection that's not our entry
            for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                if (i == entryDirection) continue;
                if (connections[i] == ConnectionType.Closed) {
                    return (i, string.Empty);
                }
            }
            break;

        case TurnoutStateEnum.Thrown:
            // Find the Diverging (D) connection that's not our entry
            for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                if (i == entryDirection) continue;
                if (connections[i] == ConnectionType.Diverging) {
                    return (i, string.Empty);
                }
            }
            break;
        }

        return null;
    }

    public struct ConnectionValidationResult {
        public bool IsValid { get; set; }
        public string Reason { get; set; }

        public ConnectionValidationResult(bool isValid, string reason = "") {
            IsValid = isValid;
            Reason = reason;
        }
    }

    private (TrackEntity track, int entryDirection, ConnectionValidationResult result)? FindNextTrackAndValidateConnection(TrackEntity currentTrack, int exitDirection) {
        // Calculate the position of the next track
        var offset = GetDirectionOffset(exitDirection);
        var nextCol = currentTrack.Col + offset.dx;
        var nextRow = currentTrack.Row + offset.dy;

        // Find the track at the next position
        if (currentTrack.Parent?.GetEntityAtPosition(nextCol, nextRow) is not TrackEntity nextTrack) {
            return null; // No track found
        }

        // Calculate the entry direction for the next track (opposite of our exit)
        var entryDirection = GetOppositeDirection(exitDirection);

        // Get connection types for validation - IMPORTANT: Use the rotated connections
        var currentConnections = GetTrackConnections(currentTrack);
        var nextConnections = GetTrackConnections(nextTrack);

        if (currentConnections == null || nextConnections == null) {
            return (nextTrack, entryDirection, new ConnectionValidationResult(false, "Missing connection information"));
        }

        // Get the ACTUAL connection types considering rotation
        var currentExitType = currentConnections.GetConnection(exitDirection, currentTrack.Rotation);
        var nextEntryType = nextConnections.GetConnection(entryDirection, nextTrack.Rotation);

        Console.WriteLine($"Connection check: Current track exit {exitDirection} = {currentExitType}, Next track entry {entryDirection} = {nextEntryType}");
        Console.WriteLine($"Current track rotation: {currentTrack.Rotation}°, Next track rotation: {nextTrack.Rotation}°");

        // Validate the connection based on your rules
        var validationResult = ValidateConnection(currentTrack, nextTrack, currentExitType, nextEntryType);

        return (nextTrack, entryDirection, validationResult);
    }

    private ConnectionValidationResult ValidateConnection(TrackEntity currentTrack,
                                                          TrackEntity nextTrack,
                                                          ConnectionType currentExitType,
                                                          ConnectionType nextEntryType) {
        Console.WriteLine($"Validating connection: {currentExitType} -> {nextEntryType}");
        Console.WriteLine($"Current track: ({currentTrack.Col}, {currentTrack.Row}), Next track: ({nextTrack.Col}, {nextTrack.Row})");

        // Handle None connections - this ends the path
        if (nextEntryType == ConnectionType.None) {
            return new ConnectionValidationResult(false, "No connection available");
        }

        // CRITICAL: Cannot connect TO a Terminator or Connector connection point
        // The path should stop at the current track
        if (nextEntryType == ConnectionType.Terminator) {
            Console.WriteLine("Next track has Terminator at entry point - path ends at current track");
            return new ConnectionValidationResult(false, "Path terminated at terminator");
        }

        if (nextEntryType == ConnectionType.Connector) {
            Console.WriteLine("Next track has Connector at entry point - path ends at current track");
            return new ConnectionValidationResult(false, "Path terminated at connector");
        }

        // Check basic pairing rules
        var result = (currentExitType, nextEntryType) switch {
            // Straight to Straight - always valid and continues
            (ConnectionType.Straight, ConnectionType.Straight) => new ConnectionValidationResult(true),

            // Straight to Diverging - only valid if next track is turnout in thrown state
            (ConnectionType.Straight, ConnectionType.Diverging) => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Thrown),

            // Straight to Closed - only valid if next track is turnout in closed state  
            (ConnectionType.Straight, ConnectionType.Closed) => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Closed),

            // Closed connections
            (ConnectionType.Closed, ConnectionType.Straight)  => new ConnectionValidationResult(true),
            (ConnectionType.Closed, ConnectionType.Closed)    => new ConnectionValidationResult(true),
            (ConnectionType.Closed, ConnectionType.Diverging) => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Thrown),

            // Diverging connections  
            (ConnectionType.Diverging, ConnectionType.Straight)  => new ConnectionValidationResult(true),
            (ConnectionType.Diverging, ConnectionType.Diverging) => new ConnectionValidationResult(true),
            (ConnectionType.Diverging, ConnectionType.Closed)    => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Closed),

            // From Terminator or Connector - these should not happen since they are endpoints
            // But if somehow we get here, allow connections outward
            (ConnectionType.Terminator, ConnectionType.Straight)  => new ConnectionValidationResult(true),
            (ConnectionType.Terminator, ConnectionType.Closed)    => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Closed),
            (ConnectionType.Terminator, ConnectionType.Diverging) => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Thrown),

            (ConnectionType.Connector, ConnectionType.Straight)  => new ConnectionValidationResult(true),
            (ConnectionType.Connector, ConnectionType.Closed)    => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Closed),
            (ConnectionType.Connector, ConnectionType.Diverging) => ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Thrown),

            _ => new ConnectionValidationResult(false, $"Invalid connection: {currentExitType} to {nextEntryType}")
        };

        Console.WriteLine($"Connection validation result: {result.IsValid}, Reason: {result.Reason}");
        return result;
    }

    private ConnectionValidationResult ValidateTurnoutConnection(TrackEntity track, TurnoutStateEnum requiredState) {
        if (track is not TurnoutEntity turnout) {
            return new ConnectionValidationResult(false, "D/X connection requires turnout");
        }

        return turnout.State switch {
            TurnoutStateEnum.Unknown              => new ConnectionValidationResult(false, "Turnout state unknown"),
            var state when state == requiredState => new ConnectionValidationResult(true),
            _                                     => new ConnectionValidationResult(false, $"Turnout in wrong state (required: {requiredState}, actual: {turnout.State})")
        };
    }

    private int GetOppositeDirection(int direction) {
        return (direction + 4) % EntityConnections.MaxDirections;
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
                await MainThread.InvokeOnMainThreadAsync(() => { segment.Tile.IsPath = true; });
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

        // Get the connections directly from the entity
        var startConnections = GetTrackConnections(startTrack);
        if (startConnections == null) return paths;

        // Find all valid starting directions
        var startDirections = GetValidDirections(startConnections, startTrack.Rotation);

        foreach (var direction in startDirections) {
            var path = TracePathInDirectionSync(startTrack, direction);
            if (path.Segments.Count > 0) {
                paths.Add(path);
            }
        }

        return paths;
    }

    private TracedPath TracePathInDirectionSync(TrackEntity startTrack, int startDirection) {
        var path = new TracedPath();
        var visited = new HashSet<TrackEntity>();
        var currentTrack = startTrack;
        var currentDirection = startDirection;

        while (currentTrack != null) {
            if (visited.Contains(currentTrack)) {
                path.StopReason = "Circular path detected";
                break;
            }

            visited.Add(currentTrack);

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

            var nextTrackInfo = FindNextTrackAndValidateConnection(currentTrack, exitDirection);
            if (nextTrackInfo == null) {
                path.StopReason = "No connecting track found";
                path.IsComplete = true;
                break;
            }

            var (nextTrack, entryDirection, connectionResult) = nextTrackInfo.Value;

            if (!connectionResult.IsValid) {
                path.StopReason = connectionResult.Reason;
                path.IsComplete = true;
                break;
            }

            currentTrack = nextTrack;
            currentDirection = entryDirection;
        }

        return path;
    }
}

// Usage service class (unchanged interface)
public class PathTracingService {
    public bool IsActive { get; private set; }
    private readonly TrackPathTracer _tracer = new();
    private CancellationTokenSource? _currentTracingCts;

    public List<TrackTile> RegisteredTiles => _tracer.RegisteredTiles;

    public void RegisterTile(TrackTile tile) {
        if (tile.Entity is TrackEntity entity) _tracer.RegisterTile(entity, tile);
    }

    public void UnregisterTile(TrackTile tile) {
        if (tile.Entity is TrackEntity entity) _tracer.UnregisterTile(entity);
    }

    public void ClearTileRegistry() {
        _tracer.ClearTileRegistry();
    }

    public async Task StartPathTracing(TrackTile tile) {
        if (tile.Entity is TrackEntity entity) await StartPathTracing(entity);
    }

    public async Task StartPathTracing(TrackEntity startTrack) {
        IsActive = true;
        try {
            Console.WriteLine($"Stopping any existing path tracing");
            await StopPathTracing();

            _currentTracingCts = new CancellationTokenSource();

            try {
                Console.WriteLine($"Calculating paths...");
                var paths = await _tracer.TracePathsFromTrackAsync(startTrack, _currentTracingCts.Token);
                await _tracer.AnimatePathsAsync(paths, _currentTracingCts.Token);
                Console.WriteLine($"Paths have been drawn");
            } catch (OperationCanceledException) {
                Console.WriteLine($"Path process was cancelled");
            } finally {
                Console.WriteLine($"Disposing of Path Tracing");
                _currentTracingCts?.Dispose();
                _currentTracingCts = null;
            }
        } catch (Exception ex) {
            IsActive = false;
            Console.WriteLine($"StartPathTracing has errrored: {ex.Message}");
        }
    }

    public async Task StopPathTracing() {
        IsActive = false;
        Console.WriteLine($"Stopping existing path tracing");
        if (_currentTracingCts is not null) {
            await _currentTracingCts.CancelAsync()!;
            _currentTracingCts.Dispose();
            _currentTracingCts = null;

            // Allow some time for cleanup
            await Task.Delay(100);
            Console.WriteLine($"Existing has completed");
        }
    }

    // Synchronous path tracing for route calculation
    public List<TrackPathTracer.TracedPath> GetPathsFromTrack(TrackEntity startTrack) {
        return _tracer.TracePathsFromTrackSync(startTrack);
    }
}