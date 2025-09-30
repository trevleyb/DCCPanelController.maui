namespace DCCPanelController.Clients;

public class DccClientEvent(DccClientState state, DccClientMessage? message) : EventArgs {
    public DccClientState State { get; init; } = state;
    public DccClientMessage? Message { get; init; } = message;
    public bool IsConnected => State == DccClientState.Connected;
}