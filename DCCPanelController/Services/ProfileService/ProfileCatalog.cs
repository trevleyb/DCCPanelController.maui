using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Services.ProfileService;

[Serializable]
public sealed partial class ProfileCatalog {
    public const string FileNameOnDisk = "DCCPanelController.index";

    public string Version { get; init; } = "1.0";
    public List<ProfileRef> Profiles { get; set; } = new();

    [JsonIgnore]
    public string ActiveFileName {
        get {
            if (Profiles.Count == 0) return string.Empty;
            var active = Profiles.FirstOrDefault(p => p.IsDefault);
            if (active == default) {
                var first = Profiles[0];
                SetDefault(first.FileName);
            }
            return Profiles.First(p => p.IsDefault).FileName;
        }
    }

    public ProfileRef? GetByFileName(string fileName) => Profiles.FirstOrDefault(p => p.FileName == fileName);

    public ProfileRef Upsert(Profile profile) {
        var i = Profiles.FindIndex(p => p.FileName == profile.Filename);
        if (i >= 0) {
            var updated = Profiles[i] with { ProfileName = profile.ProfileName };
            Profiles[i] = updated;
            Save();
            return updated;
        }

        var item = new ProfileRef(profile.ProfileName, profile.Filename, false);
        Profiles.Add(item);
        Save();
        return item;
    }

    public void Delete(Profile profile) {
        Profiles.RemoveAll(p => p.FileName == profile.Filename);
        EnsureOneDefault();
        Save();
    }

    public void Delete(string fileName) {
        Profiles.RemoveAll(p => p.FileName == fileName);
        Save();
    }

    public void SetDefault(string fileName) {
        for (var i = 0; i < Profiles.Count; i++) {
            var p = Profiles[i];
            Profiles[i] = p with { IsDefault = p.FileName == fileName };
        }
        Save();
    }

    public bool IsDefault(string fileName) => Profiles.FirstOrDefault(p => p.FileName == fileName).IsDefault;

    public string GetUniqueProfileName(string? desiredName) {
        var baseInput = string.IsNullOrWhiteSpace(desiredName) ? "Profile" : desiredName!.Trim();
        var existing = new HashSet<string>(Profiles.Select(p => p.ProfileName), StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(baseInput)) return baseInput;

        var m = NameRegex().Match(baseInput);
        var root = baseInput;
        var sep = baseInput.EndsWith(" ") ? "" : " ";
        var start = 2;

        if (m.Success) {
            root = m.Groups[1].Value;
            sep = m.Groups[2].Value;
            start = int.TryParse(m.Groups[3].Value, out var n) ? n + 1 : 2;
        }

        for (var i = start; i < int.MaxValue; i++) {
            var candidate = $"{root}{sep}{i}";
            if (!existing.Contains(candidate)) return candidate;
        }

        return"Profile";
    }

    public static ProfileCatalog Load() {
        var path = JsonRepository.GetStorageFilePath(FileNameOnDisk);
        if (File.Exists(path)) {
            try {
                var json = File.ReadAllText(path);
                var cat = JsonSerializer.Deserialize<ProfileCatalog?>(json, JsonOptions.Options) ?? new ProfileCatalog();
                cat.EnsureOneDefault();
                cat.Save();
                return cat;
            } catch {
                // swallow and recreate
            }
        }

        var fresh = new ProfileCatalog();
        fresh.Save();
        return fresh;
    }

    private void EnsureOneDefault() {
        if (Profiles.Count == 0) return;
        if (!Profiles.Any(p => p.IsDefault)) {
            var p = Profiles[0];
            var idx = 0;
            Profiles[idx] = p with { IsDefault = true };
        }
    }

    private void Save([CallerMemberName] string? member = "", [CallerLineNumber] int line = 0) {
        EnsureOneDefault();
        var json = JsonSerializer.Serialize(this, JsonOptions.Options);
        var path = JsonRepository.GetStorageFilePath(FileNameOnDisk);
        File.WriteAllText(path, json);
        LogHelper.Logger.LogInformation($"Saved ProfileCatalog {member}@{line}: {path}");
    }

    [GeneratedRegex(@"^(.*?)(\s*)(\d+)$")]
    private static partial Regex NameRegex();
}