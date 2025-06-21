using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class SettingsPageViewModel : Base.ConnectionViewModel {
    [ObservableProperty] private bool _supportsTurnouts;
    [ObservableProperty] private bool _supportsRoutes;
    [ObservableProperty] private bool _supportsBlocks;
    [ObservableProperty] private bool _supportsSensors;
    [ObservableProperty] private bool _supportsSignals;
    [ObservableProperty] private bool _supportsLights;
    [ObservableProperty] private Capabilities _capabilities = new Capabilities();
    
    [ObservableProperty] private bool _isJmriServer;
    [ObservableProperty] private bool _isWiThrottle;
    [ObservableProperty] private bool _isSimulator;
    public Models.DataModel.Settings Settings => Profile.Settings;

    public SettingsPageViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) { }

    [RelayCommand]
    public async Task SaveSettingsAsync() {
        var reconnect = ConnectionService.IsConnected;
        if (reconnect) await ConnectionService.DisconnectAsync();
        await Profile.SaveAsync();
        if (Settings is { ClientSettings: not null } && reconnect) await ConnectionService.ConnectAsync();
        await DisplayAlertHelper.DisplayOkAlertAsync("Success", "Settings and Profile Saved");
    }

    public void SetCapabilities() {
        if (!IsJmriServer && !IsWiThrottle) Capabilities = new Capabilities();
        else Capabilities = new Capabilities(Settings?.ClientSettings?.Capabilities ?? []);
    }
}