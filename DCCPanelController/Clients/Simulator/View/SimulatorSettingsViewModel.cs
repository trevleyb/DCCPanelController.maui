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
        SimulatorSettings.SimulateHeatbeat = false;
        SimulatorSettings.SimulateFastClock = false;
        SimulatorSettings.SimulateDisconnect = false;
        
        SimulatorSettings.SimulateToggles = false;
        SimulatorSettings.ToggleBlocks = false;
        SimulatorSettings.ToggleTurnouts = false;
        SimulatorSettings.ToggleLights = false;
        
        SimulatorSettings.FastClockRate = 0;
        SimulatorSettings.DisconnectEvery = 0;
        SimulatorSettings.HeartbeatSeconds = 0;
        SimulatorSettings.RandomFlipSeconds = 0;
    }

    [RelayCommand]
    private async Task ResetSettingsAsync() {
        SimulatorSettings.SimulateHeatbeat = true;
        SimulatorSettings.SimulateFastClock = true;
        SimulatorSettings.SimulateDisconnect = false;
        
        SimulatorSettings.ToggleBlocks = true;
        SimulatorSettings.ToggleTurnouts = true;
        SimulatorSettings.ToggleLights = true;
        
        SimulatorSettings.FastClockRate = 1.5;
        SimulatorSettings.DisconnectEvery = 300;
        SimulatorSettings.HeartbeatSeconds = 15;
        SimulatorSettings.RandomFlipSeconds = 5;
    }

}