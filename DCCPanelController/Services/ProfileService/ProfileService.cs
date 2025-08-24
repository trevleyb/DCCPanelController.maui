using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Services.ProfileService;

public class ProfileService {
    protected static Profiles? ProfilesIndex;
    public Profile? ActiveProfile { get; private set; }

    public ProfileService() {
        LoadProfiles();
    }

    /// <summary>
    /// On Load ensure we always have an active profile
    /// </summary>
    public void LoadProfiles() {
        if (ProfilesIndex != null && ActiveProfile != null) return;
        
        Console.WriteLine("Loading Profiles");
        ProfilesIndex = Profiles.LoadMetaData();
        if (ProfilesIndex is null) throw new ApplicationException("Failed to load or create Profiles");

        // Ensure there is at least one profile in the index
        if (ProfilesIndex.ProfileItem.Count == 0) {
            var created = AddNewProfile("Default", true);
            ActiveProfile = created;
            Console.WriteLine("Profiles Loaded");
            return;
        }

        // Get active filename (ensures one active if none already)
        var activeFileName = ProfilesIndex.ActiveProfileFileName;
        if (string.IsNullOrWhiteSpace(activeFileName)) {
            // Promote first to active as a last resort
            var first = ProfilesIndex.ProfileItem.First();
            var fallback = LoadProfile(first.FileName, setActive: true);
            ActiveProfile = fallback;
            Console.WriteLine("Profiles Loaded");
            return;
        }

        ActiveProfile = LoadProfile(activeFileName, setActive: true);
        if (ActiveProfile is null) throw new ApplicationException("Failed to load or create Profiles");
        Console.WriteLine("Profiles Loaded");
    }
    
    public async Task<Profile> LoadProfileAsync(ProfilesIndex profileIndex, bool setActive = true) => await LoadProfileAsync(profileIndex.FileName, setActive);
    public async Task<Profile> LoadProfileAsync(string profileName, bool setActive = true) {
        var profile = await JsonRepository.LoadAsync(profileName);
        if (setActive) {
            ProfilesIndex?.SetActive(profile);
            ActiveProfile = profile;
        }
        return profile;
    }

    public Profile LoadProfile(string profileName, bool setActive = true) {
        var profile = JsonRepository.Load(profileName);
        if (setActive) {
            ProfilesIndex?.SetActive(profile);
            ActiveProfile = profile;
        }
        return profile;
    }
    
    // Save the given profile (optionally as a new profile)
    public async Task SaveProfileAsync(Profile profile, bool setActive = true) {
        await JsonRepository.SaveAsync(profile, profile.Filename);
        ProfilesIndex?.Add(profile);
        if (setActive) {
            ProfilesIndex?.SetActive(profile);
            ActiveProfile = profile;
        }
    }

    // Save the given profile (optionally as a new profile)
    public void SaveProfile(Profile profile, bool setActive = true) {
        JsonRepository.Save(profile, profile.Filename);
        ProfilesIndex?.Add(profile);
        if (setActive) {
            ProfilesIndex?.SetActive(profile);
            ActiveProfile = profile;
        }
    }

    // Delete profile by name
    public async Task DeleteProfileAsync(Profile profile) {
        await JsonRepository.Delete(profile);
        ProfilesIndex?.Delete(profile);
        
        // If we just deleted the active profile then set a new active profile
        if (ActiveProfile?.Filename == profile.Filename) {
            if (ProfilesIndex?.ProfileItem?.Count > 0) {
                var next = ProfilesIndex.ProfileItem[0];
                ActiveProfile = await LoadProfileAsync(next.FileName, setActive: true);
            } else {
                // Create a brand new default
                ActiveProfile = await AddNewProfileAsync("Default", true);
            }
        }
    }

    // Upload a new profile (auto-renames for duplicates)
    public async Task<Profile?> UploadProfileAsync(string json) {
        var uploaded = await JsonRepository.UploadProfile(json);
        if (uploaded is not null) {
            // Uploaded already got a new GUID filename inside JsonRepository.UploadProfile
            await SaveProfileAsync(uploaded, setActive: false);
            return uploaded;
        }
        return null;
    }

    // Download a profile (get JSON string)
    public async Task<string> DownloadProfileAsync(string profileName) {
        var prof = await JsonRepository.LoadAsync(profileName);
        return JsonRepository.DownloadProfile(prof);
    }

    public async Task<string> DownloadProfileAsync(Profile profile) {
        var prof = await JsonRepository.LoadAsync(profile.Filename);
        return JsonRepository.DownloadProfile(prof);
    }

    public async Task<string> DownloadActiveProfile() {
        if (ActiveProfile != null) {
            return await DownloadProfileAsync(ActiveProfile.Filename);
        }
        return string.Empty;
    }
    
    // Add a new profile (with optional active, auto-naming)
    public async Task<Profile> AddNewProfileAsync(string profileName, bool setActive = true) {
        var profile = new Profile(profileName);
        await SaveProfileAsync(profile, setActive);
        return profile;
    }

    public Profile AddNewProfile(string profileName, bool setActive = true) {
        var profile = new Profile(profileName);
        SaveProfile(profile, setActive);
        return profile;
    }

    public async Task SaveActiveProfileAsync() {
        if (ActiveProfile != null) await SaveProfileAsync(ActiveProfile, setActive: true);
    }
}