namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Interface indicating that this TrackPiece is a turnout and supports either a Straight or Diverging state
/// </summary>
public interface ITrackTurnout : ITrackInteractive {
    void SetTurnoutState(TurnoutStateEnum state);
    void ExecTurnoutState(TurnoutStateEnum state);

}