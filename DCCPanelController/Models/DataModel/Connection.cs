using CommunityToolkit.Mvvm.ComponentModel;
using DCCClients.Interfaces;

namespace DCCPanelController.Models;

public partial class Connection : ObservableObject {
    [ObservableProperty] private string _name;
    [ObservableProperty] private IDccSettings? _settings;
    
    public Connection(string name, IDccSettings? settings) {
        _name = name;
        _settings = settings;
    }
}