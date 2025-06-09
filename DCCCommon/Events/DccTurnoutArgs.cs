namespace DCCCommon.Events;

public class DccTurnoutArgs : EventArgs {
    public required string TurnoutId { get; init; }
    public required string Username { get; init;}
    public int DccAddress { get; init;}
    public bool IsThrown { get; init;}
    public bool IsClosed => !IsThrown;
    public bool IsDiverging => IsThrown;
    public bool IsStraight => IsClosed;
}