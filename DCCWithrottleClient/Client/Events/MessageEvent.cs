namespace DCCWithrottleClient.Client.Events;

public class MessageEvent(string type, string value) : EventArgs, IClientEvent {
    public string Type { get; set; } = type;
    public string Value { get; set; } = value;

    public override string ToString() {
        return $"MESSAGE: {Type}: {Value}";
    }
}