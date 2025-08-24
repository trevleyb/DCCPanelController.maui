using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

public class Profiles {
    public string Version { get; init; } = "1.0";
    public List<ProfilesIndex> ProfileItem = [];

    public string ActiveProfileFileName {
        get {
            if (ProfileItem.Count == 0) return string.Empty;
            var active = ProfileItem.FirstOrDefault(x => x.IsActive);
            if (active is null) {
                // Ensure there is always an active profile
                active = ProfileItem[0];
                active.IsActive = true;
                SaveMetaData();
            }
            return active.FileName;
        }
    }
    
    public ProfilesIndex Add(Profile profile) {
        foreach (var index in ProfileItem) {
            if (index.FileName == profile.Filename) return index; // Found Existing
        }
        var newIndex = new ProfilesIndex(profile.ProfileName, profile.Filename, false);
        ProfileItem.Add(newIndex);
        SaveMetaData();
        return newIndex;
    }

    public void Delete(Profile profile) {
        var list = ProfileItem.Where(x => x.FileName == profile.Filename).ToList();
        foreach (var index in list) ProfileItem.Remove(index);
        // Ensure one active remains if we deleted the active
        if (!ProfileItem.Any(x => x.IsActive) && ProfileItem.Count > 0) {
            ProfileItem[0].IsActive = true;
        }
        SaveMetaData();
    }

    public void SetActive(Profile profile) {
        foreach (var index in ProfileItem) {
            index.IsActive = index.FileName == profile.Filename;
        }
        SaveMetaData();   
    }

    /// <summary>
    /// Save the MetaData to storage
    /// </summary>
    private void SaveMetaData() {
        var jsonString = JsonSerializer.Serialize(this, JsonOptions.Options);
        if (string.IsNullOrEmpty(jsonString)) return;

        // IMPORTANT: use a consistent logical name ("index") - GetStorageFilePath adds ".json"
        var fileName = JsonRepository.GetStorageFilePath("index");
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        File.WriteAllText(fileName, jsonString);
    }

    /// <summary>
    /// Load a copy of the MetaData from storage
    /// </summary>
    public static Profiles LoadMetaData() {
        var fileName = JsonRepository.GetStorageFilePath("index");
        if (File.Exists(fileName)) {
            try {
                var jsonString = File.ReadAllText(fileName);
                var indexFile = JsonSerializer.Deserialize<Profiles?>(jsonString, JsonOptions.Options) ?? new Profiles();
                // Ensure one active profile exists
                if (indexFile.ProfileItem.Count > 0 && !indexFile.ProfileItem.Any(x => x.IsActive)) {
                    indexFile.ProfileItem[0].IsActive = true;
                    indexFile.SaveMetaData();
                }
                return indexFile;
            } catch (Exception ex) {
                Console.WriteLine($"Unable to load MetaData: {ex.Message}");
            }
        }
        // No index found: create a fresh one with a default entry, mark it active
        var profiles = new Profiles();
        var defaultProfile = new Profile("Default");
        profiles.ProfileItem.Add(new ProfilesIndex(defaultProfile.ProfileName, defaultProfile.Filename, true));
        profiles.SaveMetaData();
        return profiles;
    }
}

public class ProfilesIndex {
    public string ProfileName { get; set; }
    public string FileName { get; set; }
    public bool   IsActive { get; set; }
    
    public ProfilesIndex(string profileName, string fileName, bool isActive) {
        ProfileName = profileName;
        FileName = fileName;
        IsActive = isActive;
    }
}