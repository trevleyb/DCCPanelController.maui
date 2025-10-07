using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients.Simulator;

public partial class SimulatorSettings : DccClientSettings, IDccClientSettings {
    public DccClientType Type => DccClientType.Simulator;

    [JsonIgnore] public bool SetManually => false;
    [JsonIgnore] public bool SetAutomatically => true;
    [JsonIgnore] public bool SupportsManualEntries => true;
    [JsonIgnore] public bool HasValidSettings => true;
    [JsonIgnore] public List<DccClientCapability> Capabilities => [DccClientCapability.Turnouts, DccClientCapability.Routes, DccClientCapability.Lights, DccClientCapability.Blocks];

    [ObservableProperty] private bool _simulateHeatbeat;
    [ObservableProperty] private bool _simulateDisconnect;
    [ObservableProperty] private bool _simulateFastClock;
    [ObservableProperty] private bool _simulateToggles;
    
    [ObservableProperty] private bool _toggleTurnouts;
    [ObservableProperty] private bool _toggleLights;
    [ObservableProperty] private bool _toggleBlocks;
    
    [ObservableProperty] private int    _heartbeatSeconds  = 15;
    [ObservableProperty] private int    _randomFlipSeconds = 30; // Every 10 Seconds
    [ObservableProperty] private int    _disconnectEvery   = 90;
    [ObservableProperty] private int    _seedCount         = 2;
    [ObservableProperty] private double _fastClockRate     = 3;


}
