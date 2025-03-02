using DCCPanelController.Model.Tracks.Actions;

namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Interface indicating that this TrackPiece is a turnout and supports either a Straight or Diverging state
/// </summary>
public interface ITrackTurnout : ITrack, ITrackInteractive, ITrackPiece {
    string ID { get; set; }
    string Address { get; set; }
    TurnoutStateEnum State { get; set; }
    ButtonActions ButtonActions { get; }
    TurnoutActions TurnoutActions { get; }
    bool SetTurnoutState(TurnoutStateEnum state);
    bool ExecTurnoutState(TurnoutStateEnum state, ActionList actionsList);
}