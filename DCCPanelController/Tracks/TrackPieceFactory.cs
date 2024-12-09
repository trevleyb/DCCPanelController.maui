using System.Collections.ObjectModel;
using System.Reflection;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public static class TrackPieceFactory {
    public static ObservableCollection<ITrackSymbol> BuildSymbols(Panel parent) {
        var trackPieceType = typeof(ITrackSymbol);
        var assembly = Assembly.GetExecutingAssembly();
        var pieces = assembly.GetTypes().Where(type => trackPieceType.IsAssignableFrom(type) && type.IsClass);

        ObservableCollection<ITrackSymbol> pieceList = [];
        foreach (var trackPiece in pieces) {
            try {
                var track = (ITrackPiece)Activator.CreateInstance(trackPiece)!;
                track.Parent = parent;
                pieceList.Add((ITrackSymbol)track);
            } catch (Exception ex) {
                Console.WriteLine($"Could not create instance of Track {trackPiece.ToString() ?? "xxx"} due to {ex.Message}");
            }
        }
        return pieceList;
    }
}