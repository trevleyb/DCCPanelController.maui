using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients.Jmri;

public partial class JmriSettings : ObservableObject, IDccClientSettings {
    [ObservableProperty] private string _address         = "localhost";
    [ObservableProperty] private string _name            = "Unknown";
    [ObservableProperty] private double _pollingInterval = 1.0;
    [ObservableProperty] private int    _port            = 12080;
    [ObservableProperty] private string _protocol        = "http";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SetManually))]
    private bool _setAutomatically;

    public JmriSettings() : this("", "", 12080) { }

    public JmriSettings(string? name, string? address, int? port) {
        Name = name ?? Environment.MachineName ?? "DCCPanelController";
        Address = address ?? "192.168.1.1";
        Port = port ?? 12080;
    }

    [JsonIgnore] public bool SetManually => !SetAutomatically;
    [JsonIgnore] public bool SupportsManualEntries => false;
    [JsonIgnore] public List<DccClientCapability> Capabilities => [DccClientCapability.Turnouts, DccClientCapability.Routes, DccClientCapability.Lights, DccClientCapability.Blocks];
    public DccClientType Type => DccClientType.Jmri;

    [JsonIgnore]
    public bool HasValidSettings {
        get {
            if (SetAutomatically) return true;
            if (string.IsNullOrEmpty(Address)) return false;
            return true;
        }
    }
}