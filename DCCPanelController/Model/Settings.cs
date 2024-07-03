using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class Settings : ObservableObject {

    [ObservableProperty] private bool _demoMode;
    [ObservableProperty] private WiServer _wiServer;
    
    public Settings() {
        var thisIp = DCCWiThrottleClient.ServiceHelper.ServiceHelper.GetLocalIPAddress();
        _wiServer = new WiServer("WiThrottle", thisIp, 12090);
    }

    public Settings(string ipAddress = "0.0.0.0", int port = 12090) {
        _wiServer = new WiServer("WiThrottle", ipAddress, 12090);
    }
}