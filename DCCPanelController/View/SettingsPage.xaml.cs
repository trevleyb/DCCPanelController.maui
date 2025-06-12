using System.ComponentModel;
using CommunityToolkit.Maui.Storage;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Models.DataModel.Repository;
using DCCPanelController.View.Settings;
using DCCPanelController.View.Settings.Jmri;
using DCCPanelController.View.Settings.WiThrottle;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private Dictionary<DccClientType, IDccClientSettings> _settingsCache = [];
    private readonly SettingsViewModel? _viewModel;

    public SettingsPage(SettingsViewModel viewModel) {
        _viewModel = viewModel;
        ArgumentNullException.ThrowIfNull(_viewModel);
        BindingContext = _viewModel;
        PropertyChanged += OnPropertyChanged;
        InitializeComponent();
        
        switch (_viewModel?.Settings?.ClientSettings?.Type) {
        case DccClientType.Jmri:
            CheckSettingsCache<JmriSettings>(DccClientType.Jmri, _viewModel?.Settings?.ClientSettings);
            _viewModel!.IsJmriServer = true;
            _viewModel!.IsWiThrottle = false;
            break;

        case DccClientType.WiThrottle:
            CheckSettingsCache<JmriSettings>(DccClientType.WiThrottle, _viewModel?.Settings?.ClientSettings);
            _viewModel!.IsJmriServer = false;
            _viewModel!.IsWiThrottle = true;
            break;

        default:
            _viewModel!.IsJmriServer = false;
            _viewModel!.IsWiThrottle = false;
            break;
        }
        _viewModel.SetCapabilities();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    protected override async void OnDisappearing() {
        base.OnDisappearing();
        if (_viewModel is { } vm) await _viewModel.Profile.SaveAsync();
    }

    private void About_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void Instructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }

    private async void Upload_OnClicked(object? sender, EventArgs e) {
        try {
            var fileName = await PromptUserForConfigFile();

            if (!string.IsNullOrEmpty(fileName)) {
                if (_viewModel is { Settings: not null } vm) {
                    var loadedJson = await LoadJsonFromFile(fileName);
                    var profile = JsonRepository.UploadSettings(loadedJson);
                    vm.Profile = profile;
                    await vm.SaveSettingsAsync();
                    await DisplayAlert("Success", "File Loaded.", "OK");
                } else {
                    throw new Exception("File could not be loaded.");
                }
            }
        } catch (Exception ex) {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void Download_OnClicked(object? sender, EventArgs e) {
        try {
            var filePath = await PromptUserForSaveLocation();

            if (!string.IsNullOrEmpty(filePath)) {
                if (_viewModel is { } vm) {
                    var saveFile = Path.Combine(filePath, "dccpanel.settings");
                    var jsonString = JsonRepository.DownloadProfile(vm.Profile);
                    await SaveJsonToFile(saveFile, jsonString);
                    await DisplayAlert("Success", "File Downloaded.", "OK");
                    Console.WriteLine(saveFile);
                }
            }
        } catch (Exception ex) {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private static async Task<string> PromptUserForConfigFile() {
        var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select the Config file to upload" });
        if (result is not null) {
            return result.FullPath;
        }
        return string.Empty;
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

    private void CheckChanged_Jmri(object? sender, CheckedChangedEventArgs e) {
        if (_viewModel is {IsJmriServer: true} ) LoadSettingsPage();
    }

    private void CheckChanged_WiThrottle(object? sender, CheckedChangedEventArgs e) {
        if (_viewModel is {IsWiThrottle: true} ) LoadSettingsPage();
    }

    private void LoadSettingsPage() {
        if (_viewModel?.Settings is null) return;

        ContentView? view = null;

        if (_viewModel.IsJmriServer) {
            _viewModel.Settings.ClientSettings = CheckSettingsCache<JmriSettings>(DccClientType.Jmri);
            view = new JmriSettingsView(_viewModel.Settings.ClientSettings, _viewModel.ConnectionService);
        } else if (_viewModel.IsWiThrottle) {
            _viewModel.Settings.ClientSettings = CheckSettingsCache<WiThrottleSettings>(DccClientType.WiThrottle);
            view = new WiThrottleSettingsView(_viewModel.Settings.ClientSettings, _viewModel.ConnectionService);
        }
        if (view is not null) SettingsView.Content = view;
        _viewModel.SetCapabilities();
    }

    private void SettingsViewOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        OnPropertyChanged();
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
}