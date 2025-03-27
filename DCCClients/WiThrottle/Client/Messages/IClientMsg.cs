using DCCClients.WiThrottle.Client.Events;

namespace DCCClients.WiThrottle.Client.Messages;

public interface IClientMsg {
    List<IClientEvent> FoundEvents { get; }
    bool HasEvents { get; }
}