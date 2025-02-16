using CommunityToolkit.Mvvm.ComponentModel;
using DCCWithrottleClient.ServiceHelper;

namespace DCCPanelController.Model;

public partial class Settings : ObservableObject {
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private bool _useConnection;
    [ObservableProperty] private WiServer _wiServer;

    public Settings() {
        var thisIp = ServiceHelper.GetLocalIPAddress();
        _wiServer = new WiServer("WiThrottle", thisIp);
    }

    public Settings(string ipAddress = "0.0.0.0", int port = 12090) {
        _wiServer = new WiServer("WiThrottle", ipAddress);
    }
}