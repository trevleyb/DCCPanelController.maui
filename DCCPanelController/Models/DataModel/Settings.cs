using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Simulator;

namespace DCCPanelController.Models.DataModel;

public partial class Settings : ObservableObject {
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private bool _connectOnStartup = true;
    [ObservableProperty] private bool _setTurnoutStatesOnStartup = true;
    [ObservableProperty] private IDccClientSettings? _clientSettings = new SimulatorSettings();
}