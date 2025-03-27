namespace DCCClients.Events;

public class DccErrorArgs : EventArgs {
    public string Error { get; }
    public bool ClientIsRunning { get; }

    public DccErrorArgs(string error, bool clientIsRunning) {
        Error = error;
        ClientIsRunning = clientIsRunning;
        throw new NotImplementedException();
    }
}