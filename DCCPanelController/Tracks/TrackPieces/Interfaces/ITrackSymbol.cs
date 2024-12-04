namespace DCCPanelController.Tracks.TrackPieces.Interfaces;

public interface ITrackSymbol {
    string Name { get; }
    ImageSource? Symbol { get; }
}