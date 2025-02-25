namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Interface to indicate that the TrackPiece supports a toggle button
/// </summary>
public interface ITrackButton : ITrack, ITrackInteractive {
    string ButtonID { get; set; }
    void SetButtonState(ButtonStateEnum state);
    void ExecButtonState(ButtonStateEnum state);
}