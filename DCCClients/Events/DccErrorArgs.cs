namespace DCCClients.Events;

public class DccErrorArgs : EventArgs {
    public DccErrorArgs(string error, bool clientIsRunning) {
        Error = error;
        ClientIsRunning = clientIsRunning;
        Console.WriteLine(error);
    }

    public string Error { get; }
    public bool ClientIsRunning { get; }
}