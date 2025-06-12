namespace DccClients.WiThrottle.Client.Events;

public class RosterEvent : EventArgs, IClientEvent {
    public override string ToString() {
        return "ROSTER: ";
    }
}