using System.Diagnostics;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Jmri.View;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.Simulator.View;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Clients.WiThrottle.View;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using DCCPanelController.View.Components;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

// Message the OperateView can listen to
public sealed class ClientTypeChangedMessage : ValueChangedMessage<DccClientType> {
    public ClientTypeChangedMessage(DccClientType value) : base(value) { }
}

public sealed class ProfileChangedMessage : ValueChangedMessage<Profile> {
    public ProfileChangedMessage(Profile value) : base(value) { }
}

public partial class UnifiedSettingsViewModel : BaseViewModel {
    private readonly Dictionary<DccClientType, IDccClientSettings> _settingsCache = [];

    public readonly ProfileService    ProfileService;
    public readonly ConnectionService ConnectionService;

    [ObservableProperty] private Profile _profile = new("Temp");
    [ObservableProperty] private bool    _isDirty;
    [ObservableProperty] private bool    _isWide;               // for responsive layout
    [ObservableProperty] private bool    _hasNotes;             // for responsive layout
    [ObservableProperty] private int     _selectedSegmentIndex; // 0 Sim, 1 JMRI, 2 WiThrottle

    [ObservableProperty] private Microsoft.Maui.Controls.View? _currentSettingsView;

    public bool IsProfileDefault => ProfileService.IsDefault(Profile);
    public bool IsProfileNotDefault => !IsProfileDefault;
    public Settings? Settings => Profile?.Settings;
    public bool CanDeleteProfile => ProfileService.NumberOfProfiles > 1;

    public UnifiedSettingsViewModel(ProfileService profileService, ConnectionService connectionService) {
        ProfileService = profileService;
        ConnectionService = connectionService;
        OnProfileChanged();
    }

    public void OnProfileChanged() {
        if (Profile is { } pOld) {
            pOld.PropertyChanged -= MarkDirty;
            pOld.Settings.PropertyChanged -= MarkDirty;
        }

        Profile = ProfileService.ActiveProfile ?? throw new("Active profile not set");
        Profile.PropertyChanged += MarkDirty;
        Profile.Settings.PropertyChanged += MarkDirty;

        // initialize selector index based on saved type
        SelectedSegmentIndex = (Profile.Settings.ClientSettings?.Type ?? DccClientType.Simulator) switch {
            DccClientType.Simulator  => 0,
            DccClientType.Jmri       => 1,
            DccClientType.WiThrottle => 2,
            _                        => 0
        };

        SetActiveSettings(IndexToType(SelectedSegmentIndex), notify: false);
        IsDirty = false;
        HasNotes = !string.IsNullOrEmpty(Profile.ProfileNotes);
        OnPropertyChanged(nameof(Settings));
        OnPropertyChanged(nameof(IsProfileDefault));
        OnPropertyChanged(nameof(IsProfileNotDefault));
        OnPropertyChanged(nameof(CanDeleteProfile));
    }

    private void MarkDirty(object? s, System.ComponentModel.PropertyChangedEventArgs e) => IsDirty = true;

    partial void OnSelectedSegmentIndexChanged(int value) {
        var type = IndexToType(value);
        SetActiveSettings(type, notify: true);
        IsDirty = true;
    }

    private static DccClientType IndexToType(int idx) => idx switch {
        0 => DccClientType.Simulator,
        1 => DccClientType.Jmri,
        2 => DccClientType.WiThrottle,
        _ => DccClientType.Simulator
    };

    // Cache/attach the appropriate settings object and view
    public void SetActiveSettings(DccClientType type, bool notify) {
        if (Settings is null) return;

        Settings.ClientSettings = GetOrCreateSettings(type, Settings.ClientSettings);
        CurrentSettingsView = BuildClientSettingsView(Settings.ClientSettings);

        if (notify) {
            WeakReferenceMessenger.Default.Send(new ClientTypeChangedMessage(type));
        }
    }

    private IDccClientSettings GetOrCreateSettings(DccClientType type, IDccClientSettings? incoming) {
        if (_settingsCache.TryGetValue(type, out var cached)) return cached;

        IDccClientSettings created = type switch {
            DccClientType.Jmri       => incoming is { Type: DccClientType.Jmri } ? incoming : new JmriSettings(),
            DccClientType.WiThrottle => incoming is { Type: DccClientType.WiThrottle } ? incoming : new WiThrottleSettings(),
            _                        => incoming is { Type: DccClientType.Simulator } ? incoming : new SimulatorSettings(),
        };
        _settingsCache[type] = created;
        return created;
    }

    private Microsoft.Maui.Controls.View BuildClientSettingsView(IDccClientSettings settings) {
        return settings.Type switch {
            DccClientType.Jmri       => new JmriSettingsView(settings, ConnectionService),
            DccClientType.WiThrottle => new WiThrottleSettingsView(settings, ConnectionService),
            _                        => new SimulatorSettingsView(settings, ConnectionService),
        };
    }

    [RelayCommand]
    public async Task SaveAsync() {
        await ProfileService.SaveAsync();
        await DisplayAlertHelper.DisplayToastAlert("Settings saved");
        IsDirty = false;
    }

    [RelayCommand] public async Task SwitchProfileAsync() { /* same logic you already have */
        await SaveAsync();
        var choices = ProfileService.GetProfileNamesWithDefault();
        var index = await ProfileSelector.ShowProfileSelector("Select a Profile",choices);
        if (index is{ } selectedProfile and>= 0) await ProfileService.SwitchProfileByIndexAsync(selectedProfile);
        OnProfileChanged();
        await DisplayAlertHelper.DisplayToastAlert("Switched Active Profile");
    }

    [RelayCommand] public async Task ActiveProfileChangedAsync() { /* same logic you already have */
        await MarkActiveProfileDefault();
        await SaveAsync();
        OnProfileChanged();
        await DisplayAlertHelper.DisplayToastAlert("Profile is now Default");
    }

    [RelayCommand] public async Task AddProfileAsync() { /* same logic you already have */
        await ProfileService.SaveAsync();
        var choices = new[] { "New Profile", "Chelsea & Balmain Sample", "Piedmont Southern Sample" };
        var index = await ProfileSelector.ShowProfileSelector("Select a Template",choices);
        if (index is { } selectedProfile and>= 0) {
            await ProfileService.SaveAsync();
            _ = selectedProfile switch {
                1 => await ProfileService.CreateFromTemplateAsync("ChelseaAndBalmain"),
                2 => await ProfileService.CreateFromTemplateAsync("PiedmontSouthern"),
                _ => await ProfileService.CreateAsync("New Profile")
            };
            OnProfileChanged();
            await DisplayAlertHelper.DisplayToastAlert("New Profile Created");
        }
    }

    [RelayCommand] public async Task DeleteProfileAsync() { /* same logic you already have */
        if (ProfileService.NumberOfProfiles <= 1) {
            await DisplayAlertHelper.DisplayToastAlert($"You cannot delete the only profile.");
            return;
        }

        var profileName = Profile?.ProfileName;
        var result = await DisplayAlertHelper.DisplayAlertAsync("Delete Profile?", "This will delete the current profile. Are you sure you want to do this?", "Continue", "Cancel");
        if (result && Profile is { } profile) {
            await ProfileService.DeleteAsync(profile);
            await DisplayAlertHelper.DisplayToastAlert($"Profile '{profileName}' Deleted");
            OnProfileChanged();
        }
        await ProfileService.SaveAsync();
    }

    [RelayCommand] public async Task UploadSettingsAsync() { /* same logic you already have */
        try {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Upload Profile?", "This will upload and add a profile from a previously stored profile.", "Continue", "Cancel");
            if (result) {
                await ProfileService.SaveAsync();
                var fileName = await PromptUserForConfigFile();
                if (!string.IsNullOrEmpty(fileName)) {
                    var loadedJson = await LoadJsonFromFileAsync(fileName);
                    await ProfileService.UploadProfileAsync(loadedJson);
                    await DisplayAlertHelper.DisplayToastAlert("Success: File Loaded");
                } else {
                    await DisplayAlertHelper.DisplayToastAlert("Error: Unable to Upload Profile");

                }
            }
        } catch (Exception ex) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"An error occurred: {ex.Message}");
        }
    }

    [RelayCommand] public async Task DownloadSettingsAsync() { /* same logic you already have */
        try {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Download Profile?", "This will replace the active profile with a previously stored profile.", "Continue", "Cancel");
            if (result) {
                await ProfileService.SaveAsync();
                var saveFile = $"{Profile?.ProfileName}.profile.json";
                var jsonBytes = await ProfileService.DownloadProfileAsync(Profile);
                if (jsonBytes is { }) {
                    var location = await FileHelper.ShareFileAsync("Save Profile", jsonBytes, saveFile);
                    await DisplayAlertHelper.DisplayToastAlert("Success: Profile Downloaded");
                    Debug.WriteLine($"Profile Saved to: {saveFile}");
                } else {
                    await DisplayAlertHelper.DisplayToastAlert("Error: Unable to download the Profile");
                }
            }
        } catch (Exception ex) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"An error occurred: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ShowAboutAsync() => await AboutPage.ShowAbout();

    public async Task MarkActiveProfileDefault() {
        if (!ProfileService.IsDefault(Profile)) ProfileService.MarkAsDefault(Profile);
        await SaveAsync();
    }
    
    #region Upload and Download Helpers
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
    #endregion
}