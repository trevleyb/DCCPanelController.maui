namespace DCCClients.Events;

public class DccRouteArgs(string dccAddress, string routeId, bool isActive) : EventArgs {
    public string DccAddress { get; } = dccAddress;
    public string RouteId { get; } = routeId;
    public bool IsActive { get; } = isActive;
    public bool IsInActive => !IsActive;
}