using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients.WiThrottle;

namespace DCCPanelController.Clients.Simulator;

public partial class SimulatorSettings : ObservableObject, IDccClientSettings {

    public bool SetManually => false;
    public bool SetAutomatically => true;
    public bool SupportsManualEntries => true;
    public List<DccClientCapability> Capabilities => SimulatorProxy.Capabilities;
    public DccClientType Type => DccClientType.Simulator;
}