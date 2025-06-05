using System.ComponentModel;
using CommunityToolkit.Maui.Storage;
using DccClients.Jmri.Client;
using DccClients.WiThrottle.Client;
using DCCCommon.Client;
using DCCPanelController.Models.DataModel.Repository;
using DCCPanelController.View.Settings;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private readonly SettingsViewModel? _viewModel;

    public SettingsPage(SettingsViewModel viewModel) {
        _viewModel = viewModel;
        ArgumentNullException.ThrowIfNull(_viewModel);
        
        BindingContext = _viewModel;
        InitializeComponent();
        switch (_viewModel?.Settings?.ClientSettings?.Type) {
            case DccClientType.Jmri: 
                _viewModel!.IsJmriServer = true;
                _viewModel!.IsWiThrottle = false;
                break;
            case DccClientType.WiThrottle: 
                _viewModel!.IsJmriServer = false;
                _viewModel!.IsWiThrottle = true;
                break;
            default:
                _viewModel!.IsJmriServer = true;
                _viewModel!.IsWiThrottle = false;
                break;
        }
    }

    protected override async void OnDisappearing() {
        base.OnDisappearing();
        if (_viewModel is {} vm) await vm.SaveSettings();
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
                    vm.SaveSettings();
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
        var result = await FolderPicker.PickAsync(new CancellationToken());
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
        if (_viewModel?.Settings?.ClientSettings?.Type != DccClientType.Jmri) LoadSettingsPage();   
    }

    private void CheckChanged_WiThrottle(object? sender, CheckedChangedEventArgs e) {
        if (_viewModel?.Settings?.ClientSettings?.Type != DccClientType.WiThrottle) LoadSettingsPage();    
    }

    private void LoadSettingsPage() {
        if (_viewModel is not null) {
            if (_viewModel.IsJmriServer && _viewModel is {Settings: not null} ) {
                if (_viewModel.Settings.ClientSettings is not JmriClientSettings) _viewModel.Settings.ClientSettings = new JmriClientSettings(); 
                var view = new JmriSettingsView(_viewModel.Settings.ClientSettings, _viewModel.ConnectionService);
                SettingsView.Content = view;
            }
            if (_viewModel.IsWiThrottle && _viewModel is {Settings: not null} ) {
                if (_viewModel.Settings.ClientSettings is not WiThrottleClientSettings) _viewModel.Settings.ClientSettings = new WiThrottleClientSettings(); 
                var view = new WiThrottleSettingsView(_viewModel.Settings.ClientSettings, _viewModel.ConnectionService);
                SettingsView.Content = view;
            }
        }
    }
}