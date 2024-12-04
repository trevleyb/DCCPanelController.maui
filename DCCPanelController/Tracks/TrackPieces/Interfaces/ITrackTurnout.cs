namespace DCCPanelController.Tracks.TrackPieces.Interfaces;

/// <summary>
/// Interface indicating that this TrackPiece is a turnout and supports either a Straight or Diverging state
/// </summary>
public interface ITrackTurnout : ITrackInteractive {
    public void Clicked();
}