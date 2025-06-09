namespace DCCCommon.Events;

public class DccSignalArgs : EventArgs {
    public required string SignalId { get; init; }
    public required string Aspect { get; init; }
}