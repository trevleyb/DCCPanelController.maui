using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCClients.WiThrottle.WiThrottle.Client;

namespace DCCPanelController.Models.DataModel;

public partial class Settings : ObservableObject {
    [ObservableProperty] private string _activeConnectionName = DeviceInfo.Name;
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private ObservableCollection<ConnectionInfo> _connections = [];
    [ObservableProperty] private bool _useConnection;

    public ConnectionInfo ActiveConnection {
        get {
            try {
                if (string.IsNullOrEmpty(ActiveConnectionName)) {
                    ActiveConnectionName = DeviceInfo.Name;
                    if (Connections.Count > 0) ActiveConnectionName = Connections[0].Name;
                }
                if (!Connections.Any(con => con.Name.Equals(ActiveConnectionName))) {
                    Connections.Add(new ConnectionInfo(DeviceInfo.Name, new WithrottleSettings(DeviceInfo.Name, "0.0.0.0", 12090)));
                }
                return Connections.First(con => con.Name.Equals(ActiveConnectionName));
            } catch (Exception ex) {
                Console.WriteLine($"This should never happen as if the key does not exist, it will be added. {ex.Message}");
                throw new InvalidOperationException("Could not get the active connection.");
            }
        }
    }
}