using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Clients;

public abstract class DccClientSettings : ObservableObject {
    public int MaxRetries { get; set; } = 5;
    public int InitialBackoffMs { get; set; } = 5000;
    public double BackoffMultiplier { get; set; } = 1;
}