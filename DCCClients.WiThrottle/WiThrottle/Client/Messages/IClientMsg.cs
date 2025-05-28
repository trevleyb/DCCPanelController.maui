
using DCCClients.WiThrottle.WiThrottle.Client.Events;

namespace DCCClients.WiThrottle.WiThrottle.Client.Messages;

public interface IClientMsg {
    List<IClientEvent> FoundEvents { get; }
    bool HasEvents { get; }
}