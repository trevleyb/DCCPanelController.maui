using CommunityToolkit.Mvvm.ComponentModel;
using DCCCommon.Client;
using static DCCCommon.Client.DccClientCapabilitiesEnum;

namespace DccClients.Jmri.Client;

public partial class JmriClientSettings : ObservableObject, IDccClientSettings {

    [ObservableProperty] private string _name = "Unknown";
    [ObservableProperty] private string _address = "localhost";
    [ObservableProperty] private int _port = 12080;
    [ObservableProperty] private string _protocol = "http";
    [ObservableProperty] private double _pollingInterval = 5.0;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SetManually))]
    private bool _setAutomatically;
    public bool SetManually => !SetAutomatically;
    public bool SupportsManualEntries => false;
    
    public DccClientType Type => DccClientType.Jmri;
    public string Url => $"{Protocol}://{Address}:{Port}";
    
    public JmriClientSettings() : this(null, "192.168.1.1", 12080) { }
    public JmriClientSettings(string address, int port) : this(null, address, port) { }
    public JmriClientSettings(string? name, string address, int port) {
        Name = name ?? Environment.MachineName ?? "DCCPanelController";
        Address = address;
        Port = port;
    }
    
    public List<DccClientCapabilitiesEnum> Capabilities => [
        Turnouts, Sensors, Blocks, Routes, Lights];

}