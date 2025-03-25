using DCCWithrottleClient.Client.Events;

namespace DCCWithrottleClient.Client.Messages;

public class ClientMsg : IClientMsg {
    public List<IClientEvent> FoundEvents { get; init; } = [];
    public bool HasEvents => FoundEvents.Count > 0;

    // Helper to add an Event to the Event Queue
    // -----------------------------------------------------------
    protected IClientEvent Add(IClientEvent msg) {
        FoundEvents.Add(msg);
        return msg;
    }
}