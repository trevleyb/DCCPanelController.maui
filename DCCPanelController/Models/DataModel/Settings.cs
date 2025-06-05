using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCCommon.Client;

namespace DCCPanelController.Models.DataModel;

public partial class Settings : ObservableObject {
    [ObservableProperty] private string _activeConnectionName = DeviceInfo.Name;
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private bool _useConnection;
    [ObservableProperty] private IDccClientSettings? _clientSettings;
}