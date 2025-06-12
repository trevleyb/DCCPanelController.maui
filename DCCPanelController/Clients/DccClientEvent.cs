namespace DCCPanelController.Clients;

public class DccClientEvent(DccClientStatus status, DccClientMessage? message) : EventArgs {
    public DccClientStatus Status { get; init; } = status;
    public DccClientMessage? Message { get; init; } = message;
    public bool IsConnected => Status == DccClientStatus.Connected;
}