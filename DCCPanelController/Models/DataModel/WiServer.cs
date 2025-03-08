using CommunityToolkit.Mvvm.ComponentModel;
using DCCWithrottleClient.ServiceHelper;

namespace DCCPanelController.Models.DataModel;

public partial class WiServer(string? name = null, string? ipAddress = null, int? port = 12090) : ObservableObject {
    [ObservableProperty] private string _ipAddress = ipAddress ?? ServiceHelper.GetLocalIPAddress();
    [ObservableProperty] private string _name = name ?? "WiThrottle";
    [ObservableProperty] private int _port = port ?? 12090;

    public WiServer() : this(string.Empty, string.Empty) { }
}