using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Events;

public class TrackSelectedEvent : EventArgs {
    public ITrackPiece Track { get; set; }
    public int Taps { get; set; }
}