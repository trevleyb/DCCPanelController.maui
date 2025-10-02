using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients.Helpers;
using DCCPanelController.Services;

namespace DCCPanelController.Clients.Simulator.View;

public partial class SimulatorSettingsViewModel : SettingsViewModel {
    [ObservableProperty] private SimulatorSettings _simulatorSettings;

    public SimulatorSettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) : base(settings, connectionService) => SimulatorSettings = Settings as SimulatorSettings ?? throw new InvalidOperationException();

    private async Task InitializeAsync() => await OnRefreshServersClickedAsync();

    [RelayCommand]
    private async Task SeedTablesAsync() {
        if (ConnectionService?.ProfileService?.ActiveProfile is {} profile && SimulatorSettings.SeedCount > 0) {
            SimulatorDccClient.SeedEntities(profile, SimulatorSettings.SeedCount);
            await DisplayAlertHelper.DisplayToastAlert("Seeded Simulator Tables");
        }
    }

    [RelayCommand]
    private async Task TurnOffAllSettingsAsync() {
        SimulatorSettings.FastClockRate = 0;
        SimulatorSettings.DisconnectEvery = 0;
        SimulatorSettings.HeartbeatSeconds = 0;
        SimulatorSettings.RandomFlipSeconds = 0;
    }

    [RelayCommand]
    private async Task ResetSettingsAsync() {
        SimulatorSettings.FastClockRate = 1.2;
        SimulatorSettings.DisconnectEvery = 90;
        SimulatorSettings.HeartbeatSeconds = 15;
        SimulatorSettings.RandomFlipSeconds = 30;
    }

}