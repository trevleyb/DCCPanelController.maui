using System.Collections.ObjectModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.Helpers;

public static class TrackPointsValidator {
    private static readonly (int dX, int dY)[] Directions = new[] {
        (0, -1), // North
        (1, -1), // North-East
        (1, 0),  // East
        (1, 1),  // South-East
        (0, 1),  // South
        (-1, 1), // South-West
        (-1, 0), // West
        (-1, -1) // North-West
    };

    public static void ClearTrackPaths(ObservableCollection<ITrack>? trackPieces) => ClearTrackPaths(trackPieces?.ToList());

    public static void ClearTrackPaths(IEnumerable<ITrack>? trackPieces) {
        if (trackPieces == null) return;
        foreach (var trackPiece in trackPieces) {
            if (trackPiece.IsPath) trackPiece.IsPath = false;
        }
    }

    public static void MarkTrackPaths(ObservableCollection<ITrack> trackPieces, ITrack track) => MarkTrackPaths(trackPieces.ToList(), track);

    public static void MarkTrackPaths(List<ITrack> trackPieces, ITrack track) {
        ClearTrackPaths(trackPieces);

        var visitedTracks = new HashSet<ITrack>();
        var queue = new Queue<ITrack>();
        queue.Enqueue(track);

        while (queue.Count > 0) {
            var currentTrack = queue.Dequeue();
            if (visitedTracks.Contains(currentTrack)) continue;
            visitedTracks.Add(currentTrack);
            currentTrack.IsPath = true;
           
            // Explore all possible connections (directions)
            // -------------------------------------------------------------
            for (var direction = 0; direction < 8; direction++) {
                var connectionType = currentTrack.Connection(direction);
                if (connectionType == TrackConnectionsEnum.None) continue;

                // Determine the neighbor's position using Directions
                var (dX, dY) = Directions[direction];
                var neighborX = currentTrack.X + dX;
                var neighborY = currentTrack.Y + dY;

                // Find the neighbor track in the collection
                var neighborTrack = trackPieces.FirstOrDefault(tp => tp.X == neighborX && tp.Y == neighborY);
                if (neighborTrack == null) continue;

                // Check the opposite direction of the neighbor for a valid reciprocal connection
                var oppositeDirection = (direction + 4) % 8;
                var neighborConnection = neighborTrack.Connection(oppositeDirection);
                
                // If the neighbor is validly connected, add it to the queue for further processing
                if (IsConnected(connectionType, neighborConnection)) {
                    if (currentTrack is ITrackTurnout turnout && connectionType is TrackConnectionsEnum.Diverging or TrackConnectionsEnum.Closed) {
                        if (turnout.State == TurnoutStateEnum.Unknown) queue.Enqueue(neighborTrack);
                        if (turnout.State == TurnoutStateEnum.Closed && connectionType == TrackConnectionsEnum.Closed) queue.Enqueue(neighborTrack);
                        if (turnout.State == TurnoutStateEnum.Thrown && connectionType == TrackConnectionsEnum.Diverging) queue.Enqueue(neighborTrack);
                    } else {
                        if (neighborTrack is ITrackTurnout related && neighborConnection is TrackConnectionsEnum.Diverging or TrackConnectionsEnum.Closed) {
                            // If we are connected to a Turnout but it is thrown the wrong direction, don't enqueue it.  
                            if (related.State == TurnoutStateEnum.Unknown) queue.Enqueue(neighborTrack);
                            if (related.State == TurnoutStateEnum.Closed && neighborConnection == TrackConnectionsEnum.Closed) queue.Enqueue(neighborTrack);
                            if (related.State == TurnoutStateEnum.Thrown && neighborConnection == TrackConnectionsEnum.Diverging) queue.Enqueue(neighborTrack);
                        } else {
                            queue.Enqueue(neighborTrack);
                        }
                    }
                } 
            }
        }
    }
    
    public static bool[] GetConnectedTracksStatus(IEnumerable<ITrack> trackPieces, ITrack track, int cols, int rows) {
        var results = new bool[8];
        var enumerable = trackPieces.ToList();

        for (var i = 0; i < 8; i++) {
            results[i] = EvaluateConnection(enumerable, track, i, cols, rows);
        }

        return results;
    }

    private static bool EvaluateConnection(IEnumerable<ITrack> trackPieces, ITrack track, int direction, int cols, int rows) {
        if (track.Connection(direction) == TrackConnectionsEnum.None) return true;

        var (dX, dY) = Directions[direction];
        var neighborX = track.X + dX;
        var neighborY = track.Y + dY;

        // If the Neighbor was OFF the edge of the page and we have a connection, then this would be an error. 
        // ----------------------------------------------------------------------------------------------------
        if (neighborX >= cols || neighborY >= rows) {
            return track.Connection(direction) == TrackConnectionsEnum.None || track.Connection(direction) == TrackConnectionsEnum.Terminator;
        }

        var oppositeDirection = (direction + 4) % 8;
        var neighborTrack = trackPieces.FirstOrDefault(tp => tp.X == neighborX && tp.Y == neighborY);

        // If there is no Neighbor track, but we are expecting one, then this is not a valid connection
        // But if this is a Terminator, then the Neighbour is fine to not be there. 
        // ---------------------------------------------------------------------------------------------
        if (neighborTrack == null) return track.Connection(direction) == TrackConnectionsEnum.Terminator;

        var neighborConnection = neighborTrack.Connection(oppositeDirection);
        return IsConnected(track.Connection(direction), neighborConnection);
    }

    private static bool IsConnected(TrackConnectionsEnum source, TrackConnectionsEnum target) {
        // If noth target and source have no connections, then it is a valid connection
        // -----------------------------------------------------------------------------------
        if (source == TrackConnectionsEnum.None && target == TrackConnectionsEnum.None) return true;

        if (source != TrackConnectionsEnum.None && source != TrackConnectionsEnum.Terminator && target != TrackConnectionsEnum.None && target != TrackConnectionsEnum.Terminator) {
            return true;
        }

        // If this is a terminator, then the connection could be another terminator, or a none
        // but anything else would indicate that we have an invalid connection. 
        // -----------------------------------------------------------------------------------
        if (source == TrackConnectionsEnum.Terminator) {
            if (target == TrackConnectionsEnum.Terminator) return true;
            if (target == TrackConnectionsEnum.None) return true;
            return false;
        }

        return false;
    }
}