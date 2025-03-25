using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class WiServer(string? name = null, string? ipAddress = null, int? port = 12090) : ObservableObject {
    [ObservableProperty] private string? _ipAddress = ipAddress;
    [ObservableProperty] private string? _name = name;
    [ObservableProperty] private int? _port = port;

    public WiServer() : this(string.Empty, string.Empty) { }
}