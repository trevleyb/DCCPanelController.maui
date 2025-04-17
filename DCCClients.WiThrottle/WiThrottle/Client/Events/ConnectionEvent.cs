using System.Data;

namespace DCCClients.WiThrottle.Client.Events;

public class ConnectionEvent(string message, ConnectionState state, bool isRunning = true) : EventArgs, IClientEvent {
    public string Message { get; set; } = message;
    public ConnectionState State { get; set; } = state;
    public bool IsRunning { get; set; } = isRunning;

    public override string ToString() {
        return $"MESSAGE: {State}: Is Running={IsRunning}";
    }
}