using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients.Simulator;

public class SimulatorSettings : ObservableObject, IDccClientSettings {
    public bool SetManually => false;
    public bool SetAutomatically => true;
    public bool SupportsManualEntries => true;
    public List<DccClientCapability> Capabilities => SimulatorProxy.Capabilities;
    public DccClientType Type => DccClientType.Simulator;
    public bool HasValidSettings => true;
}