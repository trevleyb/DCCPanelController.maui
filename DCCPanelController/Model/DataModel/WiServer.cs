using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCWithrottleClient.ServiceHelper;

namespace DCCPanelController.Model.DataModel;

[JsonSerializable(typeof(WiServer))]
public partial class WiServer(string? name = null, string? ipAddress = null, int? port = 12090) : ObservableObject {
    [ObservableProperty] private string _ipAddress = ipAddress ?? ServiceHelper.GetLocalIPAddress();
    [ObservableProperty] private string _name = name ?? "WiThrottle";
    [ObservableProperty] private int _port = port ?? 12090;

    public WiServer() : this(string.Empty, string.Empty) { }
}