using CommunityToolkit.Mvvm.ComponentModel;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Discovery;

namespace DccClients.WiThrottle.Client;

public partial class WiThrottleClientSettings : ObservableObject, IDccClientSettings {

    [ObservableProperty] private string _name = "Unknown";
    [ObservableProperty] private string _address = "localhost";
    [ObservableProperty] private int _port = 12080;
    [ObservableProperty] private string _protocol = "http";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SetManually))]
    private bool _setAutomatically;
    public bool SetManually => !SetAutomatically;
    public bool SupportsManualEntries => true;
    
    public DccClientType Type => DccClientType.WiThrottle;
    public string Url => $"{Protocol}://{Address}:{Port}";
    
    public WiThrottleClientSettings() : this(null, "192.168.1.1", 12090) { }
    public WiThrottleClientSettings(string address, int port) : this(null, address, port) { }
    public WiThrottleClientSettings(string? name, string address, int port) {
        Name = name ?? Environment.MachineName ?? "DCCPanelController";
        Address = address;
        Port = port;
    }

    public Guid Id { get; init; } = Guid.NewGuid();
    public string GetNameMessage => $"N{Name}";
    public string GetHardwareMessage => $"HU{Id.ToString()}";
    public override string ToString() {
        return $"SystemName: {Name}, Address: {Address}, Port: {Port}";
    }
    
    public List<DccClientCapabilitiesEnum> Capabilities => [DccClientCapabilitiesEnum.Turnouts, DccClientCapabilitiesEnum.Routes];

}