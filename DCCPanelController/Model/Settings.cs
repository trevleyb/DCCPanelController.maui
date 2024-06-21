using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class Settings : ObservableObject {

    [ObservableProperty] private string _ipAddress;
    [ObservableProperty] private int _port = 12090;
    
    public Settings() {
        _ipAddress = "0.0.0.0";
        _port = 12090;
    }

    public Settings(string ipAddress = "0.0.0.0", int port = 12090) {
        _ipAddress = ipAddress;
        _port = port;
    }
}