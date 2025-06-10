using CommunityToolkit.Mvvm.ComponentModel;
using DCCCommon.Client;

namespace DccClients.Jmri.Client;

public partial class UnknownSettings : ObservableObject, IDccClientSettings {
    public DccClientType Type => DccClientType.Unknown;
    public bool SetAutomatically => false;
    public bool SupportsManualEntries => false;
}