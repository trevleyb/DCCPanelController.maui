using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriConnectionChangedEventArgs : System.EventArgs {
    public ConnectionStateEnum ConnectionState { get; init; } 
    public bool IsConnected => ConnectionState == ConnectionStateEnum.Connected;
    public string Message { get; init; } = string.Empty;
    public string CallerDetails { get; init; } = string.Empty;
}
