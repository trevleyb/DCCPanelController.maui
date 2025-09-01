using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients.Simulator;

public class SimulatorSettings : ObservableObject, IDccClientSettings {
    public DccClientType Type => DccClientType.Simulator;

    [JsonIgnore] public bool SetManually => false;
    [JsonIgnore] public bool SetAutomatically => true;
    [JsonIgnore] public bool SupportsManualEntries => true;
    [JsonIgnore] public List<DccClientCapability> Capabilities => SimulatorProxy.Capabilities;
    [JsonIgnore]public bool HasValidSettings => true;
}