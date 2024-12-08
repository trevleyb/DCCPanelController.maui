namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Interface to indicate that the TrackPiece supports touch or click - it is an interactive TrackPiece
/// </summary>
public interface ITrackInteractive {
    public void Clicked();
}