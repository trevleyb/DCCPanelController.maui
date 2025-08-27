using System.Runtime.CompilerServices;
using System.Text.Json;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Services.ProfileService;

[Serializable]
public partial class ProfileIndexFile {
    public const string ProfileIndexFileName = "DCCPanelController.index";

    public string Version { get; init; } = "1.0";
    public List<ProfileIndexItem> Profiles { get; set; } = [];

    public string ActiveProfileFileName {
        get {
            if (Profiles.Count == 0) return string.Empty;
            var active = Profiles.FirstOrDefault(x => x.IsDefault);
            if (active is null) {
                active = Profiles[0];
                active.IsDefault = true;
                SaveMetaData();
            }
            return active.FileName;
        }
    }

    public ProfileIndexItem? GetByFileName(string filename) {
        return Profiles.FirstOrDefault(x => x.FileName == filename) ?? null;
    }

    public ProfileIndexItem AddOrUpdate(Profile profile) {
        ProfileIndexItem? item = null;
        foreach (var index in Profiles.Where(index => index.FileName == profile.Filename)) {
            item = index;
            item.ProfileName = profile.ProfileName;
            break;
        }

        if (item is null) {
            item = new ProfileIndexItem(profile.ProfileName, profile.Filename, false);
            Profiles.Add(item);
        }
        SaveMetaData();
        return item;
    }

    public void Delete(Profile profile) {
        var list = Profiles.Where(x => x.FileName == profile.Filename).ToList();
        foreach (var index in list) Profiles.Remove(index);

        // Ensure one active remains if we deleted the active
        if (!Profiles.Any(x => x.IsDefault) && Profiles.Count > 0) {
            Profiles[0].IsDefault = true;
        }
        SaveMetaData();
    }

    public void SetAsDefault(Profile profile) {
        foreach (var index in Profiles) {
            index.IsDefault = index.FileName == profile.Filename;
        }
        SaveMetaData();
    }

    public bool IsDefault(Profile profile) {
        foreach (var index in Profiles) {
            if (index.FileName == profile.Filename) return index.IsDefault;
        }
        return false;
    }

    /// <summary>
    /// We always need to ensure there is a DEFAULT profile for when
    /// we start up. So if we save out MetaData and there is not default
    /// item, then we need to mark one as being the default. 
    /// </summary>
    public void EnsureDefaultDefined() {
        if (!Profiles.Any(index => index.IsDefault)) Profiles[0].IsDefault = true;
    }

/// <summary>
    /// Save the MetaData to storage
    /// </summary>
    private void SaveMetaData([CallerMemberName] string? memberName = "", [CallerLineNumber] int sourceLineNumber = 0 ) {
        EnsureDefaultDefined();
        var jsonString = JsonSerializer.Serialize(this, JsonOptions.Options);
        if (string.IsNullOrEmpty(jsonString)) return;

        var fileName = JsonRepository.GetStorageFilePath(ProfileIndexFileName);
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        File.WriteAllText(fileName, jsonString);
        Console.WriteLine($"Saved MetaData {memberName}@{sourceLineNumber}: {fileName}");
    }

    /// <summary>
    /// Load a copy of the MetaData from storage
    /// </summary>
    public static ProfileIndexFile LoadMetaData() {
        var fileName = JsonRepository.GetStorageFilePath(ProfileIndexFileName);
        if (File.Exists(fileName)) {
            try {
                var jsonString = File.ReadAllText(fileName);
                var indexFile = JsonSerializer.Deserialize<ProfileIndexFile?>(jsonString, JsonOptions.Options) ?? new ProfileIndexFile();
                // Ensure one active profile exists
                if (indexFile.Profiles.Count > 0 && !indexFile.Profiles.Any(x => x.IsDefault)) {
                    indexFile.Profiles[0].IsDefault = true;
                    indexFile.SaveMetaData();
                }
                return indexFile;
            } catch (Exception ex) {
                Console.WriteLine($"Unable to load MetaData: {ex.Message}");
            }
        }
        var profiles = new ProfileIndexFile();
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
        var existing = new HashSet<string>(Profiles.Select(p => p.ProfileName), StringComparer.OrdinalIgnoreCase);

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
