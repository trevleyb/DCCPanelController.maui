using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Resources.Styles;

namespace DCCPanelController.Models.DataModel;

public partial class Settings : ObservableObject {

    [ObservableProperty] private IDccClientSettings? _clientSettings  = new SimulatorSettings();
    [ObservableProperty] private Color               _foregroundColor = StyleHelper.FromStyle("White");
    [ObservableProperty] private Color               _backgroundColor = StyleHelper.FromStyle("Primary");
    [ObservableProperty] private string              _logLevel        = "Info";
    [ObservableProperty] private double              _selectorWidth   = 72;
    [ObservableProperty] private bool                _connectOnStartup;
    [ObservableProperty] private bool                _setTurnoutStatesOnStartup;
    [ObservableProperty] private bool                _showWelcomePage = true;
    [ObservableProperty] private bool                _useClickSounds  = true;

    [ObservableProperty] private TitleBarTextDisplayEnum _titleBarDisplay  = TitleBarTextDisplayEnum.PanelName;
}