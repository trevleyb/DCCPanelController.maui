namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Interface indicating that this TrackPiece is a turnout and supports either a Straight or Diverging state
/// </summary>
public interface ITrackTurnout : ITrack, ITrackInteractive, ITrackPiece {
    string TurnoutID { get; set; }
    string Address { get; set; }
    ButtonActions ButtonActions { get; }
    TurnoutActions TurnoutActions { get; }
    bool SetTurnoutState(TurnoutStateEnum state);
    bool ExecTurnoutState();
    bool ExecTurnoutState(TurnoutStateEnum state);
}