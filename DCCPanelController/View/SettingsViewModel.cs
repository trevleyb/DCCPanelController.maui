using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
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
using DCCPanelController.View.Settings.Jmri;
using DCCPanelController.View.Settings.Simulator;
using DCCPanelController.View.Settings.WiThrottle;

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

    public Models.DataModel.Settings? Settings => Profile.Settings;
    public Profile Profile => _profileService?.ActiveProfile ?? throw new ArgumentNullException(nameof(Profile),"SettingsViewModel: Active profile is not defined.");
    private readonly Dictionary<DccClientType, IDccClientSettings> _settingsCache = [];
    private readonly ProfileService _profileService;

    public SettingsPageViewModel(ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _profileService = profileService;
    }

    [RelayCommand]
    public async Task SaveSettingsAsync() {
        var reconnect = ConnectionService.IsConnected;
        if (reconnect) await ConnectionService.DisconnectAsync();
        await _profileService.SaveActiveProfileAsync();
        if (Settings is { ClientSettings: not null } && reconnect) await ConnectionService.ConnectAsync();
        await DisplayAlertHelper.DisplayOkAlertAsync("Success", "Settings and Profile Saved");
    }

    public void SetCapabilities() {
        Capabilities = new Capabilities(Settings?.ClientSettings?.Capabilities ?? []);
    }

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

    
    public void SetActiveSettings() {
        switch (Settings?.ClientSettings?.Type) {
        case DccClientType.Simulator:
            CheckSettingsCache<JmriSettings>(DccClientType.Jmri, _profileService.ActiveProfile?.Settings?.ClientSettings);
            IsSimulator = true;
            IsJmriServer = false;
            IsWiThrottle = false;
            break;

        case DccClientType.Jmri:
            CheckSettingsCache<JmriSettings>(DccClientType.Jmri, _profileService.ActiveProfile?.Settings?.ClientSettings);
            IsSimulator = false;
            IsJmriServer = true;
            IsWiThrottle = false;
            break;

        case DccClientType.WiThrottle:
            CheckSettingsCache<JmriSettings>(DccClientType.WiThrottle, _profileService.ActiveProfile?.Settings?.ClientSettings);
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
    }
    private IDccClientSettings CheckSettingsCache<T>(DccClientType type, IDccClientSettings? settings = null) where T : IDccClientSettings, new() {
        try {
            if (_settingsCache.TryGetValue(type, out var cache)) return cache;
            if (settings is not null && settings.Type == type) {
                _settingsCache[settings.Type] = settings;
                return settings;
            }
            var newSettings = new T();
            _settingsCache[type] = newSettings;
            return newSettings;
        } catch (Exception ex) {
            Console.WriteLine($"CheckSettings: {ex.Message}");
            return new T();
        }
    }

    [RelayCommand]
    private async Task UploadSettingsAsync() {
        try {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Upload Profile?", "This will replace the active profile with a previously stored profile.", "Continue", "Cancel");
            if (result) {
                var fileName = await PromptUserForConfigFile(); 
                if (!string.IsNullOrEmpty(fileName)) {
                    var loadedJson = await LoadJsonFromFile(fileName);
                    await _profileService.UploadProfileAsync(loadedJson);
                    await DisplayAlertHelper.DisplayOkAlertAsync("Success", "File Loaded.");
                } else {
                    throw new Exception("File could not be loaded.");
                }
            }
        } catch (Exception ex) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"An error occurred: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DownloadSettingsAsync() {
        try {
            var filePath = await PromptUserForSaveLocation();
            if (!string.IsNullOrEmpty(filePath)) {
                var saveFile = Path.Combine(filePath, "dccpanel.settings");
                var jsonString = _profileService.DownloadActiveProfile();
                await SaveJsonToFile(saveFile, jsonString);
                await DisplayAlertHelper.DisplayOkAlertAsync("Success", "File Downloaded.");
                Console.WriteLine(saveFile);
            }
        } catch (Exception ex) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"An error occurred: {ex.Message}");
        }
    }
    
    private static async Task<string> PromptUserForConfigFile() {
        var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select the Config file to upload" });
        return result is not null ? result.FullPath : string.Empty;
    }

    private static async Task<string> PromptUserForSaveLocation() {
        var result = await FolderPicker.PickAsync(CancellationToken.None);
        result.EnsureSuccess();
        return result.Folder.Path ?? string.Empty;
    }

    // Method to save the JSON file to the specified location
    private async Task SaveJsonToFile(string filePath, string jsonData) {
        await using var streamWriter = new StreamWriter(filePath, false);
        await streamWriter.WriteAsync(jsonData);
    }

    // Method to save the JSON file to the specified location
    private async Task<string> LoadJsonFromFile(string filePath) {
        using var reader = new StreamReader(filePath);
        return await reader.ReadToEndAsync();
    }

}