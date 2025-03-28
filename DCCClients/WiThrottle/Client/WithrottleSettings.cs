using DCCClients.Interfaces;

namespace DCCClients.WiThrottle.Client;

public class WithrottleSettings : IDccSettings {

    public string Type => "withrottle";
    public WithrottleSettings(string address, int port) : this(null, address, port) { }
    public WithrottleSettings(string? name, string address, int port) {
        Name = name ?? Environment.MachineName ?? "DCCPanelController";
        Address = address;
        Port = port;
    }

    public string? Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();

    public string GetNameMessage => $"N{Name}";
    public string GetHardwareMessage => $"HU{Id.ToString()}";

    public override string ToString() {
        return $"SystemName: {Name}, Address: {Address}, Port: {Port}";
    }
}