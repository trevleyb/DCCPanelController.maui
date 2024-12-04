using System.Reflection;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks;

public static class TrackPieceFactory {
    private static List<ITrackPiece>? _trackPieces;
    public static List<ITrackPiece> TrackPieces => _trackPieces ??= BuildTrackPieces();

    private static List<ITrackPiece> BuildTrackPieces() {
        var trackPieceType = typeof(ITrackSymbol);
        var assembly = Assembly.GetExecutingAssembly();
        var pieces = assembly.GetTypes().Where(type => trackPieceType.IsAssignableFrom(type) && type.IsClass);

        List<ITrackPiece> pieceList = [];
        foreach (var trackPiece in pieces) {
            try {
                var track = (ITrackPiece)Activator.CreateInstance(trackPiece)!;
                pieceList.Add(track);
            } catch (Exception ex) {
                Console.WriteLine($"Could not create instance of Track {trackPiece.ToString() ?? "xxx"} due to {ex.Message}");
            }
        }

        return pieceList;
    }
}