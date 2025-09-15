using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.PathFinder;

public class TrackPathTracer {
    // Dictionary to look up tiles by their track entities (optional for highlighting)
    private readonly Dictionary<TrackEntity, TrackTile> _trackToTileLookup = new();
    public void RegisterTile(TrackEntity track, TrackTile tile) => _trackToTileLookup[track] = tile;
    public void UnregisterTile(TrackEntity track) => _trackToTileLookup.Remove(track);
    public void ClearTileRegistry() => _trackToTileLookup.Clear();

    public List<TrackTile> RegisteredTiles() => _trackToTileLookup.Values.ToList();

    public async Task<List<TracedPath>> TracePathsFromTrackAsync(TrackEntity startTrack,
        CancellationToken cancellationToken = default) {
        var paths = new List<TracedPath>();

        // Get the connections directly from the entity
        // --------------------------------------------------------------
        var startConnections = GetTrackConnections(startTrack);
        if (startConnections == null) return paths;

        // Find all valid starting directions (non-None connections)
        // --------------------------------------------------------------
        var startDirections = GetValidDirections(startConnections, startTrack.Rotation);
        foreach (var direction in startDirections.TakeWhile(direction => !cancellationToken.IsCancellationRequested)) {
            var path = await TracePathInDirectionAsync(
                startTrack,
                direction,
                cancellationToken);

            if (path.Segments.Count > 0) paths.Add(path);
        }
        return paths;
    }

    private async Task<TracedPath> TracePathInDirectionAsync(TrackEntity startTrack,
        int startDirection,
        CancellationToken cancellationToken) {
        var path = new TracedPath();
        var visited = new HashSet<TrackEntity>();
        var currentDirection = startDirection;
        var currentTrack = startTrack;

        while (!cancellationToken.IsCancellationRequested) {
            // Check if we've already visited this track in this path
            // ------------------------------------------------------
            if (!visited.Add(currentTrack)) {
                path.StopReason = "Circular path detected";
                break;
            }

            // Get connections for the current track from the entity
            // ------------------------------------------------------
            var connections = GetTrackConnections(currentTrack);
            if (connections == null) {
                path.StopReason = "No connection information available";
                break;
            }

            // Find the exit direction
            // ------------------------------------------------------
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
            // -------------------
            var currentTile = _trackToTileLookup.GetValueOrDefault(currentTrack);
            path.Segments.Add(new PathSegment(currentTrack, currentTile, currentDirection, exitDirection));

            if (!string.IsNullOrEmpty(stopReason)) {
                path.StopReason = stopReason;
                path.IsComplete = true;
                break;
            }

            // Find the next track and check connection compatibility
            // ------------------------------------------------------
            var nextTrackInfo = FindNextTrackAndValidateConnection(currentTrack, exitDirection);
            if (nextTrackInfo == null) {
                path.StopReason = "No connecting track found";
                path.IsComplete = true;
                break;
            }

            var (nextTrack, entryDirection, connectionResult) = nextTrackInfo.Value;

            if (!connectionResult.IsValid) {
                // If the reason is "Path terminated at terminator", this is a valid end
                // ---------------------------------------------------------------------
                if (connectionResult.Reason.Contains("terminated")) {
                    path.StopReason = connectionResult.Reason;
                    path.IsComplete = true;
                } else {
                    path.StopReason = connectionResult.Reason;
                    path.IsComplete = false;
                }
                break;
            }

            currentTrack = nextTrack;
            currentDirection = entryDirection;
        }
        return path;
    }

    private EntityConnections? GetTrackConnections(TrackEntity track) => track.Connections;

    private List<int> GetValidDirections(EntityConnections connections, int rotation) {
        // Get all directions that have actual connections (not None)
        // For starting directions, we can start from any connection type EXCEPT:
        // - None (no connection)
        // When determining valid starting directions, we should include all connection types
        // because the restriction on T and C is about ENTERING through them, not starting FROM them
        var validDirections = new List<int>();
        var currentConnections = connections.GetConnections(rotation);

        for (var i = 0; i < EntityConnections.MaxDirections; i++) {
            var connectionType = currentConnections[i];

            // Allow starting from any connection type that exists
            // The entry/exit restrictions will be handled in FindExitDirection and ValidateConnection
            if (connectionType != ConnectionType.None) validDirections.Add(i);
        }
        return validDirections;
    }

// TrackPathTracer.cs — replace this method
    private (int exitDirection, string stopReason)? FindExitDirection(TrackEntity track,
        EntityConnections connections,
        int entryDirection) {
        var current = connections.GetConnections(track.Rotation);
        var entryType = current[entryDirection];

        if (entryType == ConnectionType.None) return null;

        // Turnouts handled separately
        if (track is TurnoutEntity turnout)
            return HandleTurnoutExit(turnout, current, entryDirection);

        // Starting at a Terminator or Connector: allow leaving via S (start-of-path convenience)
        if (entryType == ConnectionType.Terminator || entryType == ConnectionType.Connector) {
            for (int i = 0; i < EntityConnections.MaxDirections; i++)
                if (i != entryDirection && current[i] == ConnectionType.Straight)
                    return(i, string.Empty);
            return null;
        }

        // For normal tiles: S and P are mutually exclusive channels.
        if (entryType == ConnectionType.Straight || entryType == ConnectionType.Crossing) {
            int? endDir = null;
            string? endReason = null;
            int? sameTypeDir = null;

            for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                if (i == entryDirection) continue;
                var t = current[i];

                if (t == ConnectionType.Terminator) {
                    endDir = i;
                    endReason = "Path terminated at terminator end";
                    break;
                }
                if (t == ConnectionType.Connector) {
                    endDir = i;
                    endReason = "Path terminated at connector end";
                    break;
                }
                if (t == entryType && sameTypeDir is null) {
                    sameTypeDir = i;
                }
            }

            if (endDir is int eidx) return(eidx, endReason!);
            if (sameTypeDir is int sidx) return(sidx, string.Empty);
            return null;
        }

        // Non-turnout tiles should not expose D/X internally; bail if encountered
        return null;
    }

    private bool CanConnectionsPair(ConnectionType entryType, ConnectionType exitType) =>

        // Based on your rules - these are internal connections within a single track piece
        // This determines if entry and exit points on the SAME track can connect
        (entryType, exitType) switch {
            // Straight connections - can pair with anything except Terminators and Connectors
            (ConnectionType.Straight, ConnectionType.Straight)  => true,
            (ConnectionType.Crossing, ConnectionType.Crossing)  => true,
            (ConnectionType.Straight, ConnectionType.Closed)    => true,
            (ConnectionType.Straight, ConnectionType.Diverging) => true,

            // Closed (X) connections - can pair with Straight and other closed
            (ConnectionType.Closed, ConnectionType.Closed)    => true,
            (ConnectionType.Closed, ConnectionType.Straight)  => true,
            (ConnectionType.Closed, ConnectionType.Crossing)  => true,
            (ConnectionType.Closed, ConnectionType.Diverging) => true,

            // Diverging (D) connections - can pair with Straight and other diverging  
            (ConnectionType.Diverging, ConnectionType.Diverging) => true,
            (ConnectionType.Diverging, ConnectionType.Straight)  => true,
            (ConnectionType.Diverging, ConnectionType.Crossing)  => true,
            (ConnectionType.Diverging, ConnectionType.Closed)    => true,

            // IMPORTANT: Terminators and Connectors are ENDPOINTS
            // You can ENTER them from S, but you CANNOT EXIT from them
            // They do NOT pair with anything as an exit point
            (ConnectionType.Terminator, _) => false, // Cannot exit from Terminator
            (ConnectionType.Connector, _)  => false, // Cannot exit from Connector

            // You also cannot ENTER through a Terminator or Connector
            (_, ConnectionType.Terminator) => false, // Cannot enter through Terminator  
            (_, ConnectionType.Connector)  => false, // Cannot enter through Connector

            _ => false,
        };

// TrackPathTracer.cs — replace this method
    private (int exitDirection, string stopReason)? HandleTurnoutExit(TurnoutEntity turnout,
        ConnectionType[] connections,
        int entryDirection) {
        var entryType = connections[entryDirection];

        switch (turnout.State) {
            case TurnoutStateEnum.Unknown:
                return(entryDirection, "Turnout state unknown");

            case TurnoutStateEnum.Closed:
                // S -> X, X -> S
                if (entryType == ConnectionType.Straight) {
                    for (int i = 0; i < EntityConnections.MaxDirections; i++)
                        if (i != entryDirection && connections[i] == ConnectionType.Closed)
                            return(i, string.Empty);
                } else if (entryType == ConnectionType.Closed) {
                    for (int i = 0; i < EntityConnections.MaxDirections; i++)
                        if (i != entryDirection && connections[i] == ConnectionType.Straight)
                            return(i, string.Empty);
                }
            break;

            case TurnoutStateEnum.Thrown:
                // S -> D, D -> S
                if (entryType == ConnectionType.Straight) {
                    for (int i = 0; i < EntityConnections.MaxDirections; i++)
                        if (i != entryDirection && connections[i] == ConnectionType.Diverging)
                            return(i, string.Empty);
                } else if (entryType == ConnectionType.Diverging) {
                    for (int i = 0; i < EntityConnections.MaxDirections; i++)
                        if (i != entryDirection && connections[i] == ConnectionType.Straight)
                            return(i, string.Empty);
                }
            break;
        }

        // Crossing (P) doesn't participate in a turnout; no valid exit in that case
        return null;
    }

    private (TrackEntity track, int entryDirection, ConnectionValidationResult result)? FindNextTrackAndValidateConnection(TrackEntity currentTrack, int exitDirection) {
        // Calculate the position of the next track
        var offset = EntityConnections.GetDirectionOffset(exitDirection);
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
            return(nextTrack, entryDirection, new ConnectionValidationResult(false, "Missing connection information"));
        }

        // Get the ACTUAL connection types considering rotation
        var currentExitType = currentConnections.GetConnection(exitDirection, currentTrack.Rotation);
        var nextEntryType = nextConnections.GetConnection(entryDirection, nextTrack.Rotation);

        // Validate the connection based on rules
        var validationResult = ValidateConnection(currentTrack, nextTrack, currentExitType, nextEntryType);
        return(nextTrack, entryDirection, validationResult);
    }

    private ConnectionValidationResult ValidateConnection(TrackEntity currentTrack,
        TrackEntity nextTrack,
        ConnectionType currentExitType,
        ConnectionType nextEntryType) {
        // Hard stops on the *next* tile's entry
        switch (nextEntryType) {
            case ConnectionType.None:       return new(false, "No connection available");
            case ConnectionType.Terminator: return new(false, "Path terminated at terminator");
            case ConnectionType.Connector:  return new(false, "Path terminated at connector");
        }

        // --- Inter-tile standard joins ---
        // S <-> S
        if (currentExitType == ConnectionType.Straight && nextEntryType == ConnectionType.Straight)
            return new(true);

        // P <-> P
        if (currentExitType == ConnectionType.Crossing && nextEntryType == ConnectionType.Crossing)
            return new(true);

        // NEW: permit lane change ACROSS tiles
        if (currentExitType == ConnectionType.Straight && nextEntryType == ConnectionType.Crossing)
            return new(true);
        if (currentExitType == ConnectionType.Crossing && nextEntryType == ConnectionType.Straight)
            return new(true);

        // --- Entering a turnout from S or P ---
        if ((currentExitType == ConnectionType.Straight || currentExitType == ConnectionType.Crossing)
         && nextEntryType == ConnectionType.Diverging)
            return ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Thrown);

        if ((currentExitType == ConnectionType.Straight || currentExitType == ConnectionType.Crossing)
         && nextEntryType == ConnectionType.Closed)
            return ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Closed);

        // --- Leaving a turnout toward non-turnout: D|X -> S or P ---
        if ((currentExitType == ConnectionType.Diverging || currentExitType == ConnectionType.Closed)
         && (nextEntryType == ConnectionType.Straight || nextEntryType == ConnectionType.Crossing))
            return new(true);

        // --- Turnout-to-turnout joins on branches (state must match) ---
        if (currentExitType == ConnectionType.Diverging && nextEntryType == ConnectionType.Diverging)
            return ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Thrown);
        if (currentExitType == ConnectionType.Closed && nextEntryType == ConnectionType.Closed)
            return ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Closed);
        if (currentExitType == ConnectionType.Diverging && nextEntryType == ConnectionType.Closed)
            return ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Closed);
        if (currentExitType == ConnectionType.Closed && nextEntryType == ConnectionType.Diverging)
            return ValidateTurnoutConnection(nextTrack, TurnoutStateEnum.Thrown);

        return new(false, $"Invalid connection: {currentExitType} to {nextEntryType}");
    }

    private ConnectionValidationResult ValidateTurnoutConnection(TrackEntity track, TurnoutStateEnum requiredState) {
        if (track is not TurnoutEntity turnout) {
            return new ConnectionValidationResult(false, "D/X connection requires turnout");
        }

        return turnout.State switch {
            TurnoutStateEnum.Unknown              => new ConnectionValidationResult(false, "Turnout state unknown"),
            var state when state == requiredState => new ConnectionValidationResult(true),
            _                                     => new ConnectionValidationResult(false, $"Turnout in wrong state (required: {requiredState}, actual: {turnout.State})"),
        };
    }

    private static int GetOppositeDirection(int direction) => (direction + 4) % EntityConnections.MaxDirections;

    public struct ConnectionValidationResult(bool isValid, string reason = "") {
        public bool IsValid { get; set; } = isValid;
        public string Reason { get; set; } = reason;
    }
}