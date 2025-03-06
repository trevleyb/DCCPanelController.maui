using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCWithrottleClient.ServiceHelper;

namespace DCCPanelController.Model.DataModel;

[JsonSerializable(typeof(Settings))]
public partial class Settings : ObservableObject {
    [ObservableProperty] private bool _useConnection;
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private ObservableCollection<WiServer> _wiServers = [];

    public Settings() {
        var thisIp = ServiceHelper.GetLocalIPAddress();
        if (WiServers.Count == 0) WiServers.Add(new WiServer("WiThrottle", thisIp));
    }

    public Settings(string ipAddress = "0.0.0.0", int port = 12090) {
        WiServers.Add(new WiServer("WiThrottle", ipAddress, port));
    }
}