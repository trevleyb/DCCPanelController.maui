using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks.Interfaces;

/// <summary>
/// Interface to indicate that the TrackPiece supports a toggle button
/// </summary>
public interface ITrackButton : ITrackInteractive, ITrackSymbol {
    public void Clicked();
}