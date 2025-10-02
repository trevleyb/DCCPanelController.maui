namespace DCCPanelController.Clients.WiThrottle.Events;

public class PowerEvent(int state) : EventArgs, IClientEvent {
    public int State { get; set; } = state;
    public string Value =>
        State switch {
            0 => "Off",
            1 => "On",
            2 => "Unknown",
            _ => "Unknown"
        };

    public override string ToString() {
        return $"POWER: {State}: {Value}";
    }
}