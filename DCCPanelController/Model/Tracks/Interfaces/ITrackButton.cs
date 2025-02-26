namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Interface to indicate that the TrackPiece supports a toggle button
/// </summary>
public interface ITrackButton : ITrack, ITrackInteractive {
    string ButtonID { get; set; }
    ButtonActions ButtonActions { get; }
    TurnoutActions TurnoutActions { get; }
    bool SetButtonState(ButtonStateEnum state);
    bool ExecButtonState();
    bool ExecButtonState(ButtonStateEnum state);
}