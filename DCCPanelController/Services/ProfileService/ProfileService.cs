using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Services;

// The profile service is a single instance class that manages Profiles. It allows 
// for them to be loaded, saved, and events raised when they change so that other views and
// solutions can adapt on changes. 
//
// Long term, this will allow for the selection of multiple profiles.
//
public partial class ProfileService : ObservableObject {
    private Profile? _activeProfile;
    private readonly Lock _profileLock = new Lock();
    private bool _isInitialized = false;
    private ILogger<ProfileService> _logger = LogHelper.CreateLogger<ProfileService>();
    
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private ObservableCollection<string> _availableProfiles = [];

    // Events for profile changes
    public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;
    public event EventHandler<ProfileDataChangedEventArgs>? ProfileDataChanged;


    public ProfileService() {
        PropertyChanged += OnActiveProfileChanged;
        _ = Task.Run(async () => InitialiseProfileService());
    }

    public async Task InitialiseProfileService() {
        try {
            await RefreshAvailableProfilesAsync();
            await LoadProfileAsync(AvailableProfiles[0]);
        } catch (Exception ex) {
            _logger.LogCritical("ProfileService: Error Initialising Service => {Message}", ex.Message);
            throw new ApplicationException($"ProfileService: Error Initialising Service", ex);
        }
    }
    
    /// <summary>
    /// Gets the currently active profile. Always returns a non-null profile.
    /// If no profile is loaded, it will create a default empty profile synchronously.
    /// </summary>
    public Profile ActiveProfile {
        get {
            lock (_profileLock) {
                if (_activeProfile == null) {
                    _activeProfile = new Profile("default");
                    SubscribeToProfileEvents(_activeProfile);
                    
                    if (!_isInitialized) {
                        _isInitialized = true;
                        _ = Task.Run(async () => await InitializeProfileAsync());
                    }
                }
                return _activeProfile;
            }
        }
        private set {
            lock (_profileLock) {
                if (SetProperty(ref _activeProfile, value)) {
                    OnPropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Initialize the profile asynchronously in the background
    /// </summary>
    private async Task InitializeProfileAsync() {
        try {
            await RefreshAvailableProfilesAsync();
            await LoadProfileAsync("default");
        } catch (Exception ex) {
            _logger.LogCritical("Failed to initialize profile: {Message}",ex.Message);
        }
    }

    /// <summary>
    /// Gets the currently active profile asynchronously, loading the default if none is set
    /// </summary>
    /// <returns>The active profile</returns>
    public async Task<Profile> GetActiveProfileAsync() {
        // If we already have a profile, return it
        if (_activeProfile != null && _isInitialized) {
            return _activeProfile;
        }

        // Load the profile if not already initialized
        if (!_isInitialized) {
            _isInitialized = true;
            await LoadProfileAsync("default");
        }

        return _activeProfile!;
    }

    /// <summary>
    /// Loads a profile by name and sets it as the active profile
    /// </summary>
    /// <param name="profileName">The name of the profile to load</param>
    public async Task LoadProfileAsync(string profileName) {
        IsLoading = true;
        try {
            var oldProfile = _activeProfile;
            Profile newProfile;

            try {
                newProfile = await Profile.LoadAsync(profileName);
                newProfile.FixLoadedPanels();
            } catch (Exception ex) {
                // If we can't load the specified profile, create a new empty one

                _logger.LogDebug("Failed to load profile '{profileName}', creating new empty profile: {Message}",profileName,ex.Message);
                newProfile = new Profile(profileName);
            }

            if (oldProfile != null) {
                UnsubscribeFromProfileEvents(oldProfile);
            }

            ActiveProfile = newProfile;
            SubscribeToProfileEvents(newProfile);
            ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(oldProfile, newProfile));
        } finally {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Ensures the profile is loaded asynchronously. Call this during app startup.
    /// </summary>
    public async Task EnsureProfileLoadedAsync() {
        if (!_isInitialized) {
            await RefreshAvailableProfilesAsync();
            await GetActiveProfileAsync();
        }
    }

    /// <summary>
    /// Reloads the current active profile
    /// </summary>
    public async Task ReloadActiveProfileAsync() {
        if (_activeProfile != null) {
            await LoadProfileAsync(_activeProfile.ProfileName);
        } else throw new ApplicationException("ProfileService: Active Profile should never be null when reloading active profile.");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Saves the current active profile
    /// </summary>
    public async Task SaveActiveProfileAsync() {
        await ActiveProfile.SaveAsync();
    }

    /// <summary>
    /// Downloads the current active profile as a JSON string
    /// </summary>
    /// <returns>JSON string representation of the profile</returns>
    public string DownloadActiveProfile() {
        return JsonRepository.DownloadProfile(ActiveProfile);
    }

    /// <summary>
    /// Uploads a new profile from JSON string
    /// </summary>
    /// <param name="jsonString">JSON string representation of the profile</param>
    /// <param name="profileName">Name for the new profile</param>
    /// <param name="setAsActive">Whether to set this as the active profile</param>
    public async Task UploadProfileAsync(string jsonString, bool setAsActive = true, string? profileName = null) {
        var profile = await JsonRepository.UploadSettingsAsync(jsonString);
        if (profileName is not null) profile.ProfileName = profileName;
        profile.FixLoadedPanels();

        await JsonRepository.SaveAsync(profile, profile.ProfileName);
        if (setAsActive) await LoadProfileAsync(profile.ProfileName);
        await RefreshAvailableProfilesAsync();
    }

    /// <summary>
    /// Overwrites an existing profile with new data
    /// </summary>
    /// <param name="jsonString">JSON string representation of the profile</param>
    /// <param name="profileName">Name of the profile to overwrite</param>
    /// <param name="setAsActive">Whether to set this as the active profile</param>
    public async Task OverwriteProfileAsync(string jsonString, string profileName, bool setAsActive = true) {
        await JsonRepository.Delete(profileName);
        await UploadProfileAsync(jsonString, setAsActive, profileName);
    }

    /// <summary>
    /// Refreshes the list of available profiles
    /// </summary>
    public async Task RefreshAvailableProfilesAsync() {
        AvailableProfiles.Clear();
        AvailableProfiles.Add("default");
    }

    /// <summary>
    /// Notifies that profile data has changed
    /// </summary>
    /// <param name="changeType">Type of change that occurred</param>
    /// <param name="changedObject">The object that changed</param>
    public void NotifyProfileDataChanged(ProfileDataChangeType changeType, object? changedObject = null) {
        ProfileDataChanged?.Invoke(this, new ProfileDataChangedEventArgs(changeType, changedObject));
    }

    private void OnActiveProfileChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ActiveProfile)) {
            // Additional logic when the active profile changes
        }
    }

    private void SubscribeToProfileEvents(Profile profile) {
        profile.PropertyChanged += OnProfilePropertyChanged;
        profile.Blocks.CollectionChanged += OnProfileCollectionChanged;
        profile.Routes.CollectionChanged += OnProfileCollectionChanged;
        profile.Signals.CollectionChanged += OnProfileCollectionChanged;
        profile.Turnouts.CollectionChanged += OnProfileCollectionChanged;
        profile.Sensors.CollectionChanged += OnProfileCollectionChanged;
        profile.Lights.CollectionChanged += OnProfileCollectionChanged;
        profile.Panels.CollectionChanged += OnProfileCollectionChanged;
    }

    private void UnsubscribeFromProfileEvents(Profile profile) {
        profile.PropertyChanged -= OnProfilePropertyChanged;
        profile.Blocks.CollectionChanged -= OnProfileCollectionChanged;
        profile.Routes.CollectionChanged -= OnProfileCollectionChanged;
        profile.Signals.CollectionChanged -= OnProfileCollectionChanged;
        profile.Turnouts.CollectionChanged -= OnProfileCollectionChanged;
        profile.Sensors.CollectionChanged -= OnProfileCollectionChanged;
        profile.Lights.CollectionChanged -= OnProfileCollectionChanged;
        profile.Panels.CollectionChanged -= OnProfileCollectionChanged;
    }

    private void OnProfilePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        NotifyProfileDataChanged(ProfileDataChangeType.PropertyChanged, sender);
    }

    private void OnProfileCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        NotifyProfileDataChanged(ProfileDataChangeType.CollectionChanged, sender);
    }
}