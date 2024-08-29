using System.Collections;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;
using NetworkExtension;

namespace DCCPanelController.Tracks.Helpers;

public static class TrackPointsValidator {
    
    private static readonly (int dX, int dY)[] Directions = new (int, int)[] {
        (0, -1), // North
        (1, -1), // North-East
        (1, 0),  // East
        (1, 1),  // South-East
        (0, 1),  // South
        (-1, 1), // South-West
        (-1, 0), // West
        (-1, -1) // North-West
    };

    public static bool[] GetConnectedTracksStatus(IEnumerable<ITrackPiece> trackPieces, ITrackPiece trackPiece, int cols, int rows) {
        var results = new bool[8];
        for (var i = 0; i < 8; i++) {
            results[i] = EvaluateConnection(trackPieces, trackPiece, i, cols, rows);
        }
        return results;
    }

    private static bool EvaluateConnection(IEnumerable<ITrackPiece> trackPieces, ITrackPiece trackPiece, int direction, int cols, int rows) {
        var rotatedPoints = trackPiece.Connections;
        if (rotatedPoints[direction] == TrackConnectionsEnum.None) return true;
        
        var (dX, dY) = Directions[direction];
        var neighborX = trackPiece.X + dX;
        var neighborY = trackPiece.Y + dY;

        // If the Neighbor was OFF the edge of the page and we have a connection, then this would be an error. 
        // ----------------------------------------------------------------------------------------------------
        if (neighborX >= cols || neighborY >= rows) {
            return (rotatedPoints[direction] == TrackConnectionsEnum.None || rotatedPoints[direction] == TrackConnectionsEnum.Terminator);
        } 
        
        var oppositeDirection = (direction + 4) % 8;
        var neighborTrack = trackPieces.FirstOrDefault(tp => tp.X == neighborX && tp.Y == neighborY);
       
        // If there is no Neighbor track, but we are expecting one, then this is not a valid connection
        // But if this is a Terminator, then the Neighbour is fine to not be there. 
        // ---------------------------------------------------------------------------------------------
        if (neighborTrack == null) return trackPiece.Connections[direction] == TrackConnectionsEnum.Terminator;

        var neighborConnection = neighborTrack.Connections[oppositeDirection];
        return IsConnected(trackPiece.Connections[direction], neighborConnection);
    }

    private static bool IsConnected(TrackConnectionsEnum source, TrackConnectionsEnum target) {

        // If noth target and source have no connections, then it is a valid connection
        // -----------------------------------------------------------------------------------
        if (source == TrackConnectionsEnum.None && target == TrackConnectionsEnum.None) return true;

        if (source != TrackConnectionsEnum.None && source != TrackConnectionsEnum.Terminator && 
            target != TrackConnectionsEnum.None && target != TrackConnectionsEnum.Terminator) return true;
        
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