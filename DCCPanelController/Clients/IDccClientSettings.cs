namespace DCCPanelController.Clients;

public interface IDccClientSettings {
    bool SetAutomatically { get; }
    bool SetManually { get; }
    bool SupportsManualEntries { get; }
    bool HasValidSettings { get; }

    public int MaxRetries { get; set; }
    public int InitialBackoffMs { get; set; }
    public double BackoffMultiplier { get; set; }

    DccClientType Type { get; }
    List<DccClientCapability> Capabilities { get; }
}