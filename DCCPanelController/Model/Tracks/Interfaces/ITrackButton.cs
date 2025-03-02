using DCCPanelController.Model.Tracks.Actions;

namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Interface to indicate that the TrackPiece supports a toggle button
/// </summary>
public interface ITrackButton : ITrack, ITrackInteractive {
    string ID { get; set; }
    ButtonStateEnum State { get; set; }
    ButtonActions ButtonActions { get; }
    TurnoutActions TurnoutActions { get; }
    bool SetButtonState(ButtonStateEnum state);
    bool ExecButtonState(ButtonStateEnum state, ActionList actionsList);
}