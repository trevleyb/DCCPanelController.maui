using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients.WiThrottle;

public partial class WiThrottleSettings : ObservableObject, IDccClientSettings {
    [ObservableProperty] private string _address = "localhost";

    [ObservableProperty] private string _name     = "Unknown";
    [ObservableProperty] private int    _port     = 12080;
    [ObservableProperty] private string _protocol = "http";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SetManually))]
    private bool _setAutomatically;

    public WiThrottleSettings() : this("", "", 12090) { }

    public WiThrottleSettings(string? name, string? address, int? port) {
        Name = name ?? Environment.MachineName ?? "DCCPanelController";
        Address = address ?? "192.168.1.1";
        Port = port ?? 12090;
    }

    [JsonIgnore] public bool SetManually => !SetAutomatically;
    [JsonIgnore] public bool SupportsManualEntries => true;
    [JsonIgnore] public List<DccClientCapability> Capabilities => [DccClientCapability.Turnouts, DccClientCapability.Routes];

    public DccClientType Type => DccClientType.WiThrottle;

    [JsonIgnore]
    public bool HasValidSettings {
        get {
            if (SetAutomatically) return true;
            if (string.IsNullOrEmpty(Address)) return false;
            return true;
        }
    }
}