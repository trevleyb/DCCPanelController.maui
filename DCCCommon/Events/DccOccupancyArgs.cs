namespace DCCCommon.Events;

public class DccOccupancyArgs: EventArgs {
    public required string BlockId { get; init;}
    public required string UserName { get; init; }
    public required string Sensor { get; init; }
    public bool IsOccupied { get; init;}
    public bool IsFree => !IsOccupied;
}