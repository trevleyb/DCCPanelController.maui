using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class WiServer : ObservableObject {
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _ipAddress;
    [ObservableProperty] private int _port;

    public WiServer(string name, string ipAddress, int port = 12090) {
        _name = name;
        _ipAddress = ipAddress;
        _port = port;
    }
}