namespace DCCCommon.Events;

public class DccSensorArgs: EventArgs {
    public required string SensorId { get; init;}
    public required string UserName { get; init; }
    public bool IsOccupied { get; init;}
    public bool IsFree => !IsOccupied;
}