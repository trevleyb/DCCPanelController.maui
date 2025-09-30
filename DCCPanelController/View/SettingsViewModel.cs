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

public partial class SettingsPageViewModel : ConnectionViewModel {
    private readonly             ILogger<SettingsViewModel>                    _logger;
    private readonly             Dictionary<DccClientType, IDccClientSettings> _settingsCache = [];
    public readonly              ProfileService                                ProfileService;
    [ObservableProperty] private Capabilities                                  _capabilities = new();
    [ObservableProperty] private Microsoft.Maui.Controls.View?                 _currentSettingsView;

    [NotifyPropertyChangedFor(nameof(IsNavigationDrawerClosed))]
    [ObservableProperty] private bool _isNavigationDrawerOpen;

    [ObservableProperty] private Profile _profile;
    public                       bool    IsDirty;

    public SettingsPageViewModel(ILogger<SettingsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profile = new Profile("Temporary");
        ProfileService = profileService;
        OnProfileChanged();
    }

    public bool IsNavigationDrawerClosed => !IsNavigationDrawerOpen;
    public bool CanDeleteProfile => ProfileService.NumberOfProfiles > 1;
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
        OnPropertyChanged(nameof(CanDeleteProfile));
        OnPropertyChanged(nameof(IsNavigationDrawerOpen));
        OnPropertyChanged(nameof(IsProfileNotDefault));
        OnPropertyChanged(nameof(IsProfileDefault));

        OnPropertyChanged(nameof(Profile.ProfileName));
        OnPropertyChanged(nameof(Profile.Settings));
        OnPropertyChanged(nameof(Profile.Settings.BackgroundColor));
        OnPropertyChanged(nameof(Profile.Settings.ShowWelcomePage));
        OnPropertyChanged(nameof(Profile.Settings.UseClickSounds));
        OnPropertyChanged(nameof(Profile.Settings.ConnectOnStartup));
        OnPropertyChanged(nameof(Profile.Settings.SetTurnoutStatesOnStartup));

        IsDirty = false;
    }

    [RelayCommand]
    public async Task SaveSettingsAsync() {
        var reconnect = ConnectionService.ConnectionState;
        if (reconnect == DccClientStatus.Connected) await ConnectionService.DisconnectAsync();
        await ProfileService.SaveAsync();
        if (Settings is { ClientSettings: { } } && reconnect == DccClientStatus.Connected) await ConnectionService.ConnectAsync();
        await DisplayAlertHelper.DisplayToastAlert("Success: Settings and Profile Saved");
        IsDirty = false;
    }

    [RelayCommand]
    public async Task SwitchProfileAsync() {
        await SaveSettingsAsync();
        var choices = ProfileService.GetProfileNamesWithDefault();
        var index = await ProfileSelector.ShowProfileSelector(choices);
        if (index is{ } selectedProfile and>= 0) await ProfileService.SwitchProfileByIndexAsync(selectedProfile);
        OnProfileChanged();
        await DisplayAlertHelper.DisplayToastAlert("Switched Active Profile");
    }

    [RelayCommand]
    public async Task ShowAboutAsync() => await AboutPage.ShowAbout();

    [RelayCommand]
    public async Task AddProfileAsync() {
        var result = await DisplayAlertHelper.DisplayAlertAsync("Add New Profile?", "This will add a new Profile to the system and make it active. Do you wish to continue?", "Continue", "Cancel");
        if (result) {
            await SaveSettingsAsync();
            _ = await ProfileService.CreateAsync();
            await SaveSettingsAsync();
            OnProfileChanged();
            await DisplayAlertHelper.DisplayToastAlert("New Profile Created");
        }
    }

    [RelayCommand]
    public async Task DeleteProfileAsync() {
        if (ProfileService.NumberOfProfiles <= 1 || Profile is null) return;
        var profileName = Profile?.ProfileName;
        var result = await DisplayAlertHelper.DisplayAlertAsync("Delete Profile?", "This will delete the current profile. Are you sure you want to do this?", "Continue", "Cancel");
        if (result && Profile is { } profile) {
            await ProfileService.DeleteAsync(profile);
            await DisplayAlertHelper.DisplayToastAlert($"Profile '{profileName}' Deleted");
            OnProfileChanged();
        }
    }

    /// <summary>
    ///     If this profile is already the default, then there is no changed.
    ///     If this one is not the default, then we will mark it as the default
    /// </summary>
    public async Task MarkActiveProfileDefault() {
        if (!ProfileService.IsDefault(Profile)) ProfileService.MarkAsDefault(Profile);
        await SaveSettingsAsync();
    }

    [RelayCommand]
    private async Task UploadSettingsAsync() {
        try {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Upload Profile?", "This will replace the active profile with a previously stored profile.", "Continue", "Cancel");
            if (result) {
                await SaveSettingsAsync();
                var fileName = await PromptUserForConfigFile();
                if (!string.IsNullOrEmpty(fileName)) {
                    var loadedJson = await LoadJsonFromFileAsync(fileName);
                    await ProfileService.UploadProfileAsync(loadedJson);
                    await DisplayAlertHelper.DisplayToastAlert("Success: File Loaded");
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
            var result = await DisplayAlertHelper.DisplayAlertAsync("Download Profile?", "This will replace the active profile with a previously stored profile.", "Continue", "Cancel");
            if (result) {
                await SaveSettingsAsync();
                var saveFile = $"{Profile?.ProfileName}.profile.json";
                var jsonBytes = await ProfileService.DownloadProfileZipAsync(Profile);
                var location = await FileHelper.ShareFileAsync("Save Profile", jsonBytes, saveFile);
                await DisplayAlertHelper.DisplayToastAlert("Success: Profile Downloaded");
                Debug.WriteLine($"Profile Saved to: {saveFile}");
            }
        } catch (Exception ex) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"An error occurred: {ex.Message}");
        }
    }

    private static async Task<string> PromptUserForConfigFile() {
        var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select the Config file to upload" });
        return result is { } ? result.FullPath : string.Empty;
    }

    private static async Task<string> PromptUserForSaveLocation() {
        var result = await FolderPicker.PickAsync(CancellationToken.None);
        result.EnsureSuccess();
        return result.Folder.Path ?? string.Empty;
    }

    private async Task SaveJsonToFileAsync(string filePath, string jsonData) {
        await using var streamWriter = new StreamWriter(filePath, false);
        await streamWriter.WriteAsync(jsonData);
    }

    private async Task SaveJsonToFileAsync(string filePath, ReadOnlyMemory<byte> jsonData, CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        if (jsonData.IsEmpty) return;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await fileStream.WriteAsync(jsonData, cancellationToken).ConfigureAwait(false);
        await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> LoadJsonFromFileAsync(string filePath) {
        using var reader = new StreamReader(filePath);
        return await reader.ReadToEndAsync();
    }
}