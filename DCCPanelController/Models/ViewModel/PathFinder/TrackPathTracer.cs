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

    public List<TrackTile> RegisteredTiles() {
        return _trackToTileLookup.Values.ToList();
    }

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

    private (int exitDirection, string stopReason)? FindExitDirection(TrackEntity track,
                                                                      EntityConnections connections,
                                                                      int entryDirection) {
        // Get the current connections for this track's rotation
        var currentConnections = connections.GetConnections(track.Rotation);

        // Get the entry connection type
        var entryConnectionType = currentConnections[entryDirection];

        // Verify we can actually enter from this direction
        if (entryConnectionType == ConnectionType.None) return null; 

        // Special case: If we are STARTING from a Terminator or Connector connection point,
        // this means we're beginning our trace from the endpoint of the track.
        // We need to find the paired connection to continue.
        if (entryConnectionType == ConnectionType.Terminator) {

            // Find the Straight connection that pairs with this Terminator
            for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                if (i == entryDirection) continue;
                var connectionType = currentConnections[i];
                if (connectionType == ConnectionType.Straight) {
                    return (i, string.Empty); // Continue the path through the S connection
                }
            }
            return null;
        }

        if (entryConnectionType == ConnectionType.Connector) {

            // Find the Straight connection that pairs with this Connector
            for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                if (i == entryDirection) continue;
                var connectionType = currentConnections[i];
                if (connectionType == ConnectionType.Straight) {
                    return (i, string.Empty); // Continue the path through the S connection
                }
            }
            return null;
        }

        // Handle turnouts specially
        // --------------------------------------------------------------
        if (track is TurnoutEntity turnout) {
            return HandleTurnoutExit(turnout, currentConnections, entryDirection);
        }

        // Look for exit connections - check if we're connecting to a Terminator or Connector endpoint
        // --------------------------------------------------------------
        for (var i = 0; i < EntityConnections.MaxDirections; i++) {
            if (i == entryDirection) continue; // Don't go back the same way

            var connectionType = currentConnections[i];

            // Special handling for Terminators and Connectors - they are endpoints
            if (connectionType == ConnectionType.Terminator) {
                return (i, "Path terminated at terminator end");
            }

            if (connectionType == ConnectionType.Connector) {
                return (i, "Path terminated at connector end");
            }

            // Check if this connection type can pair with our entry (normal flow)
            if (CanConnectionsPair(entryConnectionType, connectionType)) {
                return (i, string.Empty); // Continue the path
            }
        }
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
            // Handle both directions: S->X and X->S
            if (entryConnectionType == ConnectionType.Straight) {
                // Entering from S, exit via X
                for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                    if (i == entryDirection) continue;
                    if (connections[i] == ConnectionType.Closed) {
                        return (i, string.Empty);
                    }
                }
            } else if (entryConnectionType == ConnectionType.Closed) {
                // Entering from X, exit via S
                for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                    if (i == entryDirection) continue;
                    if (connections[i] == ConnectionType.Straight) {
                        return (i, string.Empty);
                    }
                }
            }
            break;

        case TurnoutStateEnum.Thrown:
            // Handle both directions: S->D and D->S
            if (entryConnectionType == ConnectionType.Straight) {
                // Entering from S, exit via D
                for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                    if (i == entryDirection) continue;
                    if (connections[i] == ConnectionType.Diverging) {
                        return (i, string.Empty);
                    }
                }
            } else if (entryConnectionType == ConnectionType.Diverging) {
                // Entering from D, exit via S
                for (int i = 0; i < EntityConnections.MaxDirections; i++) {
                    if (i == entryDirection) continue;
                    if (connections[i] == ConnectionType.Straight) {
                        return (i, string.Empty);
                    }
                }
            }
            break;
        }

        return null;
    }

    public struct ConnectionValidationResult(bool isValid, string reason = "") {
        public bool IsValid { get; set; } = isValid;
        public string Reason { get; set; } = reason;
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
            return (nextTrack, entryDirection, new ConnectionValidationResult(false, "Missing connection information"));
        }

        // Get the ACTUAL connection types considering rotation
        var currentExitType = currentConnections.GetConnection(exitDirection, currentTrack.Rotation);
        var nextEntryType = nextConnections.GetConnection(entryDirection, nextTrack.Rotation);

        // Validate the connection based on rules
        var validationResult = ValidateConnection(currentTrack, nextTrack, currentExitType, nextEntryType);
        return (nextTrack, entryDirection, validationResult);
    }

    private ConnectionValidationResult ValidateConnection(TrackEntity currentTrack,
                                                          TrackEntity nextTrack,
                                                          ConnectionType currentExitType,
                                                          ConnectionType nextEntryType) {
        switch (nextEntryType) {
        case ConnectionType.None:
            return new ConnectionValidationResult(false, "No connection available");

        case ConnectionType.Terminator:
            return new ConnectionValidationResult(false, "Path terminated at terminator");

        case ConnectionType.Connector:
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

    private static int GetOppositeDirection(int direction) {
        return (direction + 4) % EntityConnections.MaxDirections;
    }

}