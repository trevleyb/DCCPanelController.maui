using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class WiServer(string? name = null, string? ipAddress = null, int? port = 12090) : ObservableObject {
    [ObservableProperty] private string _name = name ?? "WiThrottle";
    [ObservableProperty] private string _ipAddress = ipAddress ?? DCCWithrottleClient.ServiceHelper.ServiceHelper.GetLocalIPAddress();
    [ObservableProperty] private int _port = port ?? 12090;

    public WiServer() : this(string.Empty, string.Empty) { }
}

[JsonSerializable(typeof(WiServer))]
internal sealed partial class WiServerStateContext : JsonSerializerContext { }