using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients.Simulator;

public partial class SimulatorSettings : ObservableObject, IDccClientSettings {
    public DccClientType Type => DccClientType.Simulator;

    [JsonIgnore] public bool SetManually => false;
    [JsonIgnore] public bool SetAutomatically => true;
    [JsonIgnore] public bool SupportsManualEntries => true;
    [JsonIgnore] public bool HasValidSettings => true;
    [JsonIgnore] public List<DccClientCapability> Capabilities => [DccClientCapability.Turnouts, DccClientCapability.Routes, DccClientCapability.Lights, DccClientCapability.Blocks];

    public int MaxRetries { get; set; } = 5;
    public int InitialBackoffMs { get; set; } = 500;
    public double BackoffMultiplier { get; set; } = 1.5;

    [ObservableProperty] private int _heartbeatSeconds = 15;
    [ObservableProperty] private int _randomFlipSeconds = 30; // Every 10 Seconds
    [ObservableProperty] private int _burstEverySeconds = 60; // Every 30 seconds
    [ObservableProperty] private int _burstSizeMin = 2;
    [ObservableProperty] private int _burstSizeMax = 6;
    [ObservableProperty] private int _disconnectEvery = 90;
    [ObservableProperty] private double _fastClockRate  = 1.2;


}
