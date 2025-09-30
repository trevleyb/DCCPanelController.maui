namespace DCCPanelController.Clients.Jmri.Events;

public class JmriConnectionChangedEventArgs : EventArgs {
    public DccClientState ConnectionState { get; init; }
    public bool IsConnected => ConnectionState == DccClientState.Connected;
    public string Message { get; init; } = string.Empty;
    public string CallerDetails { get; init; } = string.Empty;
}