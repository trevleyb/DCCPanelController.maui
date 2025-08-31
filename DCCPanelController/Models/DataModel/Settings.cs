using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Simulator;

namespace DCCPanelController.Models.DataModel;

public partial class Settings : ObservableObject {
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private IDccClientSettings? _clientSettings = new SimulatorSettings();
    [ObservableProperty] private bool _connectOnStartup = true;
    [ObservableProperty] private string _logLevel = "Info";
    [ObservableProperty] private double _selectorWidth = 72;
    [ObservableProperty] private bool _setTurnoutStatesOnStartup = true;
    [ObservableProperty] private bool _showWelcomePage = true;
    [ObservableProperty] private bool _useClickSounds = true;
}