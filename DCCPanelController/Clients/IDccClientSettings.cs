namespace DCCPanelController.Clients;

public interface IDccClientSettings {
    bool SetAutomatically { get; }
    bool SetManually { get; }
    bool SupportsManualEntries { get; }
    
    DccClientType Type { get; }
    List<DccClientCapability> Capabilities { get; }
}
