using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients.Simulator;

public class SimulatorSettings : ObservableObject, IDccClientSettings {
    public DccClientType Type => DccClientType.Simulator;

    [JsonIgnore] public bool SetManually => false;
    [JsonIgnore] public bool SetAutomatically => true;
    [JsonIgnore] public bool SupportsManualEntries => true;
    [JsonIgnore] public bool HasValidSettings => true;
    [JsonIgnore] public List<DccClientCapability> Capabilities => [DccClientCapability.Turnouts, DccClientCapability.Routes, DccClientCapability.Lights, DccClientCapability.Blocks];

    public int HeartbeatSeconds { get; set; } = 15;
    public int RandomFlipSeconds { get; set; } = 10;       // Every 10 Seconds
    public int BurstEverySeconds { get; set; } = 30; // Every 30 seconds
    public int BurstSizeMin { get; set; } = 3;
    public int BurstSizeMax { get; set; } = 8;
    public int DisconnectEvery { get; set; } = 0;
    public double FastClockRate { get; set; } = 0;


}
