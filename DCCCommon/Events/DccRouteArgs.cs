namespace DCCCommon.Events;

public class DccRouteArgs : EventArgs {
    public required string RouteId { get; init; }
    public required string UserName { get; init; }
    public bool IsActive { get; init; }
    public bool IsInActive => !IsActive;
}