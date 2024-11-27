using DCCWithrottleClient.Client.Events;

namespace DCCWithrottleClient.Client.Messages;

public interface IClientMsg {
    List<IClientEvent> FoundEvents { get; }
    bool HasEvents { get; }
}