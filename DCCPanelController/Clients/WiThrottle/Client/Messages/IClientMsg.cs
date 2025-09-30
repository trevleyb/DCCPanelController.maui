using DCCPanelController.Clients.WiThrottle.Client.Events;

namespace DCCPanelController.Clients.WiThrottle.Client.Messages;

public interface IClientMsg {
    List<IClientEvent> FoundEvents { get; }
    bool HasEvents { get; }
}