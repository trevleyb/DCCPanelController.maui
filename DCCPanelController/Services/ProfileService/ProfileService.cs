using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Services.ProfileService;

public class ProfileService {
    protected static Profiles Profiles = new Profiles();
    public Profile? ActiveProfile { get; private set; }

    public ProfileService() {
        LoadProfiles();
    }

    public List<string> GetProfileNames() => Profiles?.ProfileItems.Select(x => x.ProfileName).ToList() ?? new List<string>();
    
    public void SetActiveProfile(Profile profile) {
        ActiveProfile = profile;
    }
    
    /// <summary>
    /// On Load ensure we always have an active profile
    /// </summary>
    public void LoadProfiles() {
        if (ActiveProfile != null) return;
        
        Console.WriteLine("Loading Profiles");
        Profiles = Profiles.LoadMetaData();
        if (Profiles is null) throw new ApplicationException("Failed to load or create Profiles");

        // Ensure there is at least one profile in the index
        if (Profiles.ProfileItems.Count == 0) {
            var created = AddNewProfile("Default", true);
            ActiveProfile = created;
            Console.WriteLine("Profiles Loaded");
            return;
        }

        // Get active filename (ensures one active if none already)
        var activeFileName = Profiles.ActiveProfileFileName;
        if (string.IsNullOrWhiteSpace(activeFileName)) {
            // Promote first to active as a last resort
            var first = Profiles.ProfileItems.First();
            var fallback = LoadProfile(first.FileName);
            ActiveProfile = fallback;
            Console.WriteLine("Profiles Loaded");
            return;
        }

        ActiveProfile = LoadProfile(activeFileName);
        if (ActiveProfile is null) throw new ApplicationException("Failed to load or create Profiles");
        Console.WriteLine("Profiles Loaded");
    }
    
    public async Task<Profile> LoadProfileAsync(ProfileIndex profileIndex, bool setActive = true) => await LoadProfileAsync(profileIndex.FileName);
    public async Task<Profile> LoadProfileAsync(string profileName) => await JsonRepository.LoadAsync(profileName);
    public Profile LoadProfile(string profileName) => JsonRepository.Load(profileName);
    
    // Save the given profile (optionally as a new profile)
    public async Task SaveProfileAsync(Profile profile, bool setActive = true) {
        await JsonRepository.SaveAsync(profile, profile.Filename);
        Profiles?.AddOrUpdate(profile);
        if (setActive) {
            Profiles?.SetActive(profile);
            ActiveProfile = profile;
        }
    }

    // Save the given profile (optionally as a new profile)
    public void SaveProfile(Profile profile, bool setActive = true) {
        JsonRepository.Save(profile, profile.Filename);
        Profiles?.AddOrUpdate(profile);
        if (setActive) {
            Profiles?.SetActive(profile);
            ActiveProfile = profile;
        }
    }

    // Delete profile by name
    public async Task DeleteProfileAsync(Profile profile) {
        await JsonRepository.Delete(profile);
        Profiles?.Delete(profile);
        
        // If we just deleted the active profile then set a new active profile
        if (ActiveProfile?.Filename == profile.Filename) {
            if (Profiles?.ProfileItems?.Count > 0) {
                var next = Profiles.ProfileItems[0];
                ActiveProfile = await LoadProfileAsync(next.FileName);
            } else {
                // Create a brand new default
                ActiveProfile = await AddNewProfileAsync("Default");
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
    public async Task<Profile> AddNewProfileAsync(string? profileName = null, bool setActive = true) {
        if (string.IsNullOrWhiteSpace(profileName)) profileName = Profiles?.GetUniqueProfileName("Profile") ?? "Profile";
        var profile = new Profile(profileName);
        await SaveProfileAsync(profile, setActive);
        return profile;
    }

    public Profile AddNewProfile(string? profileName = null, bool setActive = true) {
        if (string.IsNullOrWhiteSpace(profileName)) profileName = Profiles?.GetUniqueProfileName("Profile") ?? "Profile";
        var profile = new Profile(profileName);
        SaveProfile(profile, setActive);
        return profile;
    }

    public async Task SaveActiveProfileAsync() {
        if (ActiveProfile != null) await SaveProfileAsync(ActiveProfile, setActive: true);
    }
}