namespace DCCClients.Events;

public class DccMessageArgs(string messageType, string message) : EventArgs {
    public string MessageType { get; } = messageType;
    public string Message { get; } = message;
}