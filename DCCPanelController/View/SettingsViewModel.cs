using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DccClients.Jmri.Client;
using DccClients.WiThrottle.Client;
using DCCCommon.Client;
using DCCCommon.Discovery;
using DCCCommon.Events;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class SettingsViewModel : Base.ConnectionViewModel {
    [ObservableProperty] private bool _supportsTurnouts;
    [ObservableProperty] private bool _supportsRoutes;
    [ObservableProperty] private bool _supportsBlocks;
    [ObservableProperty] private bool _supportsSensors;
    [ObservableProperty] private bool _supportsSignals;
    [ObservableProperty] private bool _supportsLights;

    [ObservableProperty] private bool _isJmriServer;
    [ObservableProperty] private bool _isWiThrottle;
    public Models.DataModel.Settings Settings => Profile.Settings;

    public SettingsViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) { }

    [RelayCommand]
    public async Task SaveSettingsAsync() {
        var reconnect = ConnectionService.IsConnected;
        if (reconnect) await ConnectionService.DisconnectAsync();
        await Profile.SaveAsync();
        if (Settings is { ClientSettings: not null } && reconnect) await ConnectionService.ConnectAsync(Settings.ClientSettings);
        await DisplayAlertHelper.DisplayOkAlertAsync("Success", "Settings and Profile Saved");
    }

    public void SetCapabilities(IDccClientSettings? settings) {
        SupportsTurnouts = false;
        SupportsRoutes   = false;
        SupportsBlocks   = false;
        SupportsSensors  = false;
        SupportsSignals  = false;
        SupportsLights   = false;

        if (settings == null) return;
        SupportsTurnouts = settings.Capabilities.Contains(DccClientCapabilities.Turnouts);
        SupportsRoutes   = settings.Capabilities.Contains(DccClientCapabilities.Routes);
        SupportsBlocks   = settings.Capabilities.Contains(DccClientCapabilities.Blocks);
        SupportsSensors  = settings.Capabilities.Contains(DccClientCapabilities.Sensors);
        SupportsSignals  = settings.Capabilities.Contains(DccClientCapabilities.Signals);
        SupportsLights   = settings.Capabilities.Contains(DccClientCapabilities.Lights);
    }
}