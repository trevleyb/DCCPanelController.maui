using DCCPanelController.Clients.WiThrottle.Events;

namespace DCCPanelController.Clients.WiThrottle.Messages;

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