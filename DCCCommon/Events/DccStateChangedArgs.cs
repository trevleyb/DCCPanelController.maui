namespace DCCCommon.Events;

public enum ConnectionStatus {
    Connected,
    Disconnected,
    Error
}

public class DccStateChangedArgs : EventArgs {
    public DccStateChangedArgs(bool isConnected, string? message = null) {
        IsConnected= isConnected;
        Message = message ?? string.Empty;;
    }

    public bool IsConnected { get; }
    public bool IsDisconnected => !IsConnected;
    public string Message { get; }
    
}