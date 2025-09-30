using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.Simulator;

public partial class SimulatorSettingsViewModel : SettingsViewModel {
    [ObservableProperty] private SimulatorSettings _simulatorSettings;

    public SimulatorSettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) : base(settings, connectionService) => SimulatorSettings = Settings as SimulatorSettings ?? throw new InvalidOperationException();

    private async Task InitializeAsync() => await OnRefreshServersClickedAsync();
}