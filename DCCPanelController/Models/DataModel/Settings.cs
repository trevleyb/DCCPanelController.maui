using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class Settings : ObservableObject {
    [ObservableProperty] private bool _useConnection;
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private ObservableCollection<WiServer> _wiServers = [];

    public Settings() {
        if (WiServers.Count == 0) WiServers.Add(new WiServer("WiThrottle"));
    }

    public Settings(string ipAddress = "0.0.0.0", int port = 12090) {
        WiServers.Add(new WiServer("WiThrottle", ipAddress, port));
    }
}