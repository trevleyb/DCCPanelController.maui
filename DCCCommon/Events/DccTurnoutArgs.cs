namespace DCCCommon.Events;

public class DccTurnoutArgs(string dccAddress, string turnoutId, bool isThrown) : EventArgs {
    public string DccAddress { get; } = dccAddress;
    public string TurnoutId { get; } = turnoutId;
    public bool IsThrown { get; } = isThrown;
    public bool IsClosed => !IsThrown;
    public bool IsDiverging => IsThrown;
    public bool IsStraight => IsClosed;
}