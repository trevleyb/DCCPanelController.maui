using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Events;

public class TrackSelectedEvent : EventArgs {
    public ITrackPiece? Track { get; set; }
    public int Taps { get; set; }
}