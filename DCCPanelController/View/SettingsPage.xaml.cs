using System.ComponentModel;
using CommunityToolkit.Maui.Storage;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Models.DataModel.Repository;
using DCCPanelController.View.Settings;
using DCCPanelController.View.Settings.Jmri;
using DCCPanelController.View.Settings.Simulator;
using DCCPanelController.View.Settings.WiThrottle;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private readonly SettingsPageViewModel? _pageViewModel;

    public SettingsPage(SettingsPageViewModel pageViewModel) {
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        PropertyChanged += OnPropertyChanged;
        InitializeComponent();

        pageViewModel.SetActiveSettings();
        pageViewModel.SetCapabilities();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    private void About_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void Instructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new HelpPage());
    }

    // TOFIX:
    private async void Upload_OnClicked(object? sender, EventArgs e) {
    //     try {
    //         var fileName = await PromptUserForConfigFile();
    //
    //         if (!string.IsNullOrEmpty(fileName)) {
    //             if (_pageViewModel is { Settings: not null } vm) {
    //                 var loadedJson = await LoadJsonFromFile(fileName);
    //                 var profile = await JsonRepository.UploadSettingsAsync(loadedJson);
    //                 vm.Profile = profile;
    //                 await vm.SaveSettingsAsync();
    //                 await DisplayAlert("Success", "File Loaded.", "OK");
    //             } else {
    //                 throw new Exception("File could not be loaded.");
    //             }
    //         }
    //     } catch (Exception ex) {
    //         await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
    //     }
    }
    
    private async void Download_OnClicked(object? sender, EventArgs e) {
    //     try {
    //         var filePath = await PromptUserForSaveLocation();
    //
    //         if (!string.IsNullOrEmpty(filePath)) {
    //             if (_pageViewModel is { } vm) {
    //                 var saveFile = Path.Combine(filePath, "dccpanel.settings");
    //                 var jsonString = JsonRepository.DownloadProfile(vm.Profile);
    //                 await SaveJsonToFile(saveFile, jsonString);
    //                 await DisplayAlert("Success", "File Downloaded.", "OK");
    //                 Console.WriteLine(saveFile);
    //             }
    //         }
    //     } catch (Exception ex) {
    //         await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
    //     }
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
        if (_pageViewModel is { IsJmriServer: true }) {
            SettingsView.Content = _pageViewModel.LoadSettingsPage();
        }
    }

    private void CheckChanged_WiThrottle(object? sender, CheckedChangedEventArgs e) {
        if (_pageViewModel is { IsWiThrottle: true }) {
            SettingsView.Content = _pageViewModel.LoadSettingsPage();
        }
    }

    private void CheckChanged_Simulator(object? sender, CheckedChangedEventArgs e) {
        if (_pageViewModel is { IsSimulator: true }) {
            SettingsView.Content = _pageViewModel.LoadSettingsPage();
        }
    }


    private void SettingsViewOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        OnPropertyChanged();
    }
}