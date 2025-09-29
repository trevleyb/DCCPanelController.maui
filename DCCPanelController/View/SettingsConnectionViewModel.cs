using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using DCCPanelController.View.Components;
using DCCPanelController.View.Settings;
using DCCPanelController.View.Settings.Jmri;
using DCCPanelController.View.Settings.Simulator;
using DCCPanelController.View.Settings.WiThrottle;
using Microsoft.Extensions.Logging;
using Capabilities = DCCPanelController.View.Helpers.Capabilities;

namespace DCCPanelController.View;

public partial class SettingsConnectionViewModel : ConnectionViewModel {
    private readonly             ILogger<SettingsViewModel>                    _logger;
    private readonly             Dictionary<DccClientType, IDccClientSettings> _settingsCache = [];
    public readonly              ProfileService                                ProfileService;
    [ObservableProperty] private Capabilities                                  _capabilities = new();
    [ObservableProperty] private Microsoft.Maui.Controls.View?                 _currentSettingsView;

    [ObservableProperty] private bool _isJmriServer;

    [NotifyPropertyChangedFor(nameof(IsNavigationDrawerClosed))]
    [ObservableProperty] private bool _isNavigationDrawerOpen;

    [ObservableProperty] private bool    _isSimulator;
    [ObservableProperty] private bool    _isWiThrottle;
    [ObservableProperty] private Profile _profile;

    [ObservableProperty] private int  _selectedSegmentIndex;
    [ObservableProperty] private bool _supportsBlocks;
    [ObservableProperty] private bool _supportsLights;
    [ObservableProperty] private bool _supportsRoutes;
    [ObservableProperty] private bool _supportsSensors;
    [ObservableProperty] private bool _supportsSignals;
    [ObservableProperty] private bool _supportsTurnouts;
    public                       bool IsDirty;

    public SettingsConnectionViewModel(ILogger<SettingsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profile = new Profile("Temporary");
        ProfileService = profileService;
        OnProfileChanged();
    }

    public bool IsNavigationDrawerClosed => !IsNavigationDrawerOpen;
    public bool IsProfileDefault => ProfileService.IsDefault(Profile);
    public bool IsProfileNotDefault => !IsProfileDefault;
    public Models.DataModel.Settings? Settings => Profile?.Settings;

    private void ProfileOnPropertyChanged(object? sender, PropertyChangedEventArgs e) => IsDirty = true;

    public void OnProfileChanged() {
        if (Profile is { } profile) {
            Profile.PropertyChanged -= ProfileOnPropertyChanged;
            Profile.Settings.PropertyChanged -= ProfileOnPropertyChanged;
        }
        Profile = ProfileService?.ActiveProfile ?? throw new ArgumentNullException(nameof(Profile), "SettingsViewModel: Active profile is not defined.");
        Profile.PropertyChanged += ProfileOnPropertyChanged;
        Profile.Settings.PropertyChanged += ProfileOnPropertyChanged;

        OnPropertyChanged(nameof(Profile));
        OnPropertyChanged(nameof(Settings));
        OnPropertyChanged(nameof(CurrentSettingsView));
        OnPropertyChanged(nameof(Capabilities));
        OnPropertyChanged(nameof(IsNavigationDrawerOpen));
        OnPropertyChanged(nameof(IsProfileNotDefault));
        OnPropertyChanged(nameof(IsProfileDefault));
        OnPropertyChanged(nameof(Profile.ProfileName));
        OnPropertyChanged(nameof(Profile.Settings));
        IsDirty = false;
    }

    [RelayCommand]
    public async Task SaveSettingsAsync() {
        var reconnect = ConnectionService.IsConnected;
        if (reconnect) await ConnectionService.DisconnectAsync();
        await ProfileService.SaveAsync();
        if (Settings is { ClientSettings: { } } && reconnect) await ConnectionService.ConnectAsync();
        await DisplayAlertHelper.DisplayToastAlert("Success: Settings and Profile Saved");
        IsDirty = false;
    }
    
    public void SetCapabilities() => Capabilities = new Capabilities(Settings?.ClientSettings?.Capabilities ?? []);

    public Microsoft.Maui.Controls.View? LoadSettingsPage() {
        if (Settings is null) return null;
        ContentView? view = null;
        if (IsJmriServer) {
            Settings.ClientSettings = CheckSettingsCache<JmriSettings>(DccClientType.Jmri);
            view = new JmriSettingsView(Settings.ClientSettings, ConnectionService);
        } else if (IsWiThrottle) {
            Settings.ClientSettings = CheckSettingsCache<WiThrottleSettings>(DccClientType.WiThrottle);
            view = new WiThrottleSettingsView(Settings.ClientSettings, ConnectionService);
        } else if (IsSimulator) {
            Settings.ClientSettings = CheckSettingsCache<SimulatorSettings>(DccClientType.Simulator);
            view = new SimulatorSettingsView(Settings.ClientSettings, ConnectionService);
        }
        SetCapabilities();
        return view;
    }

    public void SetActiveSettings() => SetActiveSettings(Settings?.ClientSettings?.Type ?? DccClientType.Simulator);

    public void SetActiveSettings(DccClientType type) {
        switch (type) {
            case DccClientType.Simulator:
                CheckSettingsCache<SimulatorSettings>(DccClientType.Simulator, ProfileService.ActiveProfile?.Settings?.ClientSettings);
                IsSimulator = true;
                IsJmriServer = false;
                IsWiThrottle = false;
            break;

            case DccClientType.Jmri:
                CheckSettingsCache<JmriSettings>(DccClientType.Jmri, ProfileService.ActiveProfile?.Settings?.ClientSettings);
                IsSimulator = false;
                IsJmriServer = true;
                IsWiThrottle = false;
            break;

            case DccClientType.WiThrottle:
                CheckSettingsCache<WiThrottleSettings>(DccClientType.WiThrottle, ProfileService.ActiveProfile?.Settings?.ClientSettings);
                IsJmriServer = false;
                IsSimulator = false;
                IsWiThrottle = true;
            break;

            default:
                IsSimulator = true;
                IsJmriServer = false;
                IsWiThrottle = false;
            break;
        }
        SetCapabilities();
        CurrentSettingsView = LoadSettingsPage();
    }

    private IDccClientSettings CheckSettingsCache<T>(DccClientType type, IDccClientSettings? settings = null) where T : IDccClientSettings, new() {
        try {
            if (_settingsCache.TryGetValue(type, out var cache)) return cache;
            if (settings is { } && settings.Type == type) {
                _settingsCache[settings.Type] = settings;
                return settings;
            }
            var newSettings = new T();
            _settingsCache[type] = newSettings;
            return newSettings;
        } catch (Exception ex) {
            _logger.LogDebug("CheckSettings: {ExMessage}", ex.Message);
            return new T();
        }
    }
    
    // React to selection changes from the UI
    partial void OnSelectedSegmentIndexChanged(int value) {
        // Map index to type
        var type = value switch {
            0 => DccClientType.Simulator,
            1 => DccClientType.Jmri,
            2 => DccClientType.WiThrottle,
            _ => DccClientType.Simulator,
        };

        SetActiveSettings(type);
        IsDirty = true;
    }
}