using DCCPanelController.Clients.WiThrottle.Events;

namespace DCCPanelController.Clients.WiThrottle.Messages;

public interface IClientMsg {
    List<IClientEvent> FoundEvents { get; }
    bool HasEvents { get; }
}