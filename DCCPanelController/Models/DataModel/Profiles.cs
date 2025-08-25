using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

public partial class Profiles {
    public string Version { get; init; } = "1.0";
    public List<ProfileIndex> ProfileItems = [];

    public string ActiveProfileFileName {
        get {
            if (ProfileItems.Count == 0) return string.Empty;
            var active = ProfileItems.FirstOrDefault(x => x.IsActive);
            if (active is null) {
                active = ProfileItems[0];
                active.IsActive = true;
                SaveMetaData();
            }
            return active.FileName;
        }
    }

    public ProfileIndex? GetByFileName(string filename) {
        return ProfileItems.FirstOrDefault(x => x.FileName == filename) ?? null;
    }

    public ProfileIndex AddOrUpdate(Profile profile) {
        ProfileIndex? item = null;
        foreach (var index in ProfileItems.Where(index => index.FileName == profile.Filename)) {
            item = index;
            item.ProfileName = profile.ProfileName;
            break;
        }
        
        if (item is null) {
            item = new ProfileIndex(profile.ProfileName, profile.Filename, false);
            ProfileItems.Add(item);
        }
        SaveMetaData();
        return item;
    }

    public void Delete(Profile profile) {
        var list = ProfileItems.Where(x => x.FileName == profile.Filename).ToList();
        foreach (var index in list) ProfileItems.Remove(index);
        // Ensure one active remains if we deleted the active
        if (!ProfileItems.Any(x => x.IsActive) && ProfileItems.Count > 0) {
            ProfileItems[0].IsActive = true;
        }
        SaveMetaData();
    }

    public void SetActive(Profile profile) {
        foreach (var index in ProfileItems) {
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
                if (indexFile.ProfileItems.Count > 0 && !indexFile.ProfileItems.Any(x => x.IsActive)) {
                    indexFile.ProfileItems[0].IsActive = true;
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
        profiles.ProfileItems.Add(new ProfileIndex(defaultProfile.ProfileName, defaultProfile.Filename, true));
        profiles.SaveMetaData();
        return profiles;
    }
    
    // Generate a unique profile name by following the numeric pattern at the end (if any).
    // Examples:
    // - "Profile 1" -> "Profile 2" (or next free)
    // - "Profile1"  -> "Profile2"  (or next free)
    // - "My Profile" -> "My Profile" if unused, else "My Profile 2", then 3, ...
    public string GetUniqueProfileName(string? desiredName) {
        var baseInput = string.IsNullOrWhiteSpace(desiredName) ? "Profile" : desiredName!.Trim();
        var existing = new HashSet<string>(ProfileItems.Select(p => p.ProfileName), StringComparer.OrdinalIgnoreCase);

        // If it's not taken, return as-is
        if (!existing.Contains(baseInput)) return baseInput;

        // Try to detect trailing number with optional whitespace before it
        // Groups: 1=base text, 2=separator (whitespace if any), 3=digits
        var match = MyRegex().Match(baseInput);
        string baseText;
        string sep;
        int startNumber;

        if (match.Success) {
            baseText = match.Groups[1].Value;
            sep = match.Groups[2].Value; // preserve exact spacing (could be empty)
            if (!int.TryParse(match.Groups[3].Value, out var n)) n = 1;
            startNumber = n + 1;
        } else {
            baseText = baseInput;
            // If no number was present, use a single space as separator for the suffix
            sep = baseText.EndsWith(" ") ? "" : " ";
            startNumber = 2;
        }

        // Find next available number
        for (var i = startNumber; i < int.MaxValue; i++) {
            var candidate = $"{baseText}{sep}{i}";
            if (!existing.Contains(candidate)) return candidate;
        }

        // Fallback (highly unlikely)
        return $"{baseText}{sep}{Guid.NewGuid():N}".Substring(0, Math.Min(baseText.Length + sep.Length + 8, 128));
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^(.*?)(\s*)(\d+)$")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}

public class ProfileIndex {
    public string ProfileName { get; set; }
    public string FileName { get; set; }
    public bool   IsActive { get; set; }
    
    public ProfileIndex(string profileName, string fileName, bool isActive) {
        ProfileName = profileName;
        FileName = fileName;
        IsActive = isActive;
    }
}