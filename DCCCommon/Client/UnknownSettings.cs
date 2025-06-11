using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCCommon.Client;

public partial class UnknownSettings : ObservableObject, IDccClientSettings {
    public DccClientType Type => DccClientType.Unknown;
    public bool SetAutomatically => false;
    public bool SupportsManualEntries => false;
    public List<DccClientCapabilitiesEnum> Capabilities => [];

}