using DccClients.WiThrottle.Client.Events;

namespace DccClients.WiThrottle.Client.Messages;

public interface IClientMsg {
    List<IClientEvent> FoundEvents { get; }
    bool HasEvents { get; }
}