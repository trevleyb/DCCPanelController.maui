using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Services.ProfileService;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.DataModel.Repository;

public static class JsonRepository {
    private static readonly ILogger Logger = LogHelper.CreateLogger("PanelRepository");

    public static string Version { get; set; } = "1.0.1";

    public static async Task SaveAsync(Profile profile, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        try {
            var jsonString = JsonSerializer.Serialize(profile, JsonOptions.Options);
            if (string.IsNullOrEmpty(jsonString)) return;
            var fileName = GetStorageFilePath(profile.Filename);
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(profile.Filename));
            await File.WriteAllTextAsync(fileName, jsonString);
            Logger.LogInformation("Saved Data: {fileName}", fileName);
            LoggingLevelHelper.SetLogLevel(profile.Settings.LogLevel);
        } catch (Exception ex) {
            Logger.LogError(ex, "Unable to SAVE Data from {caller}:{lineNumber}", caller, lineNumber);
        }
    }

    public static void Save(Profile profile, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        try {
            var jsonString = JsonSerializer.Serialize(profile, JsonOptions.Options);
            if (string.IsNullOrEmpty(jsonString)) return;

            var fileName = GetStorageFilePath(profile.Filename);
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(profile.Filename));
            File.WriteAllText(fileName, jsonString);
            Logger.LogInformation("Saved Data: {fileName}", fileName);
            LoggingLevelHelper.SetLogLevel(profile.Settings.LogLevel);
        } catch (Exception ex) {
            Logger.LogError(ex, "Unable to SAVE Data from {caller}:{lineNumber}", caller, lineNumber);
        }
    }

    public static async Task<Profile?> LoadAsync(string profileName, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        try {
            var filePath = GetStorageFilePath(profileName);

            if (File.Exists(filePath)) {
                try {
                    var jsonString = await File.ReadAllTextAsync(filePath);
                    if (string.IsNullOrWhiteSpace(jsonString)) return null;

                    // FUTURE: Add Support for difference Schema Versions and conversion between them
                    var version = GetSchemaVersion(jsonString);
                    Logger.LogInformation("Profile Version: {version} | Repository Version: {Version}", version, Version);

                    var profile = JsonSerializer.Deserialize<Profile?>(jsonString, JsonOptions.Options) ?? throw new ApplicationException("Could not deserialize settings.");
                    LoggingLevelHelper.SetLogLevel(profile.Settings.LogLevel);
                    profile.FixLoadedPanels();
                    profile.Validate(Logger);
                    return profile;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Could not deserialize settings from {caller}:{lineNumber}. New set created", caller, lineNumber);
                    return null;
                }
            }
            Logger.LogInformation("File not found: {profileName}", profileName);
            return null;
        } catch (Exception ex) {
            Logger.LogError(ex, "Could not access Profile from {caller}:{lineNumber}. New Profile created", caller, lineNumber);
            return null;
        }
    }

    private static string GetSchemaVersion(string jsonString) {
        if (string.IsNullOrWhiteSpace(jsonString)) return"Undefined";
        try {
            var match = Regex.Match(
                jsonString,
                "\"(?i:(schemaVersion|version|Version))\"\\s*:\\s*(?:\"(?<val>(?:[^\"\\\\]|\\\\.)*)\"|(?<val>-?\\d+(?:\\.\\d+)*))",
                RegexOptions.CultureInvariant
            );
            if (!match.Success) return"Unknown";

            var raw = match.Groups["val"].Value;
            try {
                return Regex.Unescape(raw);
            } catch {
                return raw;
            }
        } catch (Exception) {
            return"Error";
        }
    }

    /// <summary>
    ///     Deletes the specified file if it exists.
    /// </summary>
    public static void Delete(Profile profile) {
        try {
            var filePath = GetStorageFilePath(profile.Filename);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
                Logger.LogInformation("Deleted profile: {fileName}", profile.Filename);
            }
        } catch (Exception ex) {
            Logger.LogWarning(ex, "Could not delete profile: {fileName}", profile.Filename);
        }
    }

    public static string DownloadProfile(Profile profile) {
        try {
            return JsonSerializer.Serialize(profile, JsonOptions.Options);
        } catch (Exception ex) {
            Logger.LogError(ex, "Could not serialize Profile for download: {fileName}", profile.Filename);
            return string.Empty;
        }
    }

    /// <summary>
    ///     Reads and deserializes the JSON data from the provided file to create and return a Storage object.
    /// </summary>
    public static async Task<Profile?> UploadProfile(string jsonString) {
        try {
            var profile = JsonSerializer.Deserialize<Profile?>(jsonString, JsonOptions.Options) ?? throw new ApplicationException("Could not deserialize settings.");

            profile.Filename = Guid.NewGuid().ToString();
            profile.FixLoadedPanels();
            await SaveAsync(profile);
            Logger.LogInformation("Uploaded profile: {fileName}", profile.Filename);
            return profile;
        } catch (Exception ex) {
            Logger.LogError(ex, "Could not deserialize uploaded profile");
            return null;
        }
    }

    private static void MigrateProfilesToNewLocation() {
        var oldStorageDir = Path.Combine(FileSystem.AppDataDirectory, "DCCPanelController");
        if (!Directory.Exists(oldStorageDir)) return;

        var newStorageDir = DirectoryHelper.GetProfileDirectory();
        try {
            var files = Directory.GetFiles(oldStorageDir, "*.json");
            if (files.Length > 0) {
                Logger.LogInformation("Migrating {count} profile files from old location to new location", files.Length);
                foreach (var file in files) {
                    try {
                        var fileName = Path.GetFileName(file);
                        var newFilePath = Path.Combine(newStorageDir, fileName);
                        if (!File.Exists(newFilePath)) {
                            File.Move(file, newFilePath);
                            Logger.LogInformation("Migrated profile: {fileName}", fileName);
                        }
                    } catch (Exception ex) {
                        Logger.LogWarning(ex, "Failed to migrate profile: {file}", file);
                    }
                }
            }
        } catch (Exception ex) {
            Logger.LogError(ex, "Error during profile migration from {oldStorageDir} to {newStorageDir}", oldStorageDir, newStorageDir);
        }
    }

    /// <summary>
    ///     Constructs and retrieves the full file path for storing the configuration file
    ///     associated with the provided profile name. Creates the necessary storage
    ///     directory if it does not already exist.
    /// </summary>
    public static string GetStorageFilePath(string profileName = "default") {
        try {
            MigrateProfilesToNewLocation();
            var file = profileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? profileName : profileName + ".json";
            return DirectoryHelper.GetProfile(file);
        } catch (Exception ex) {
            Logger.LogCritical(ex, "Unable to determine where to store the Config File for profile: {profileName}", profileName);
            throw;
        }
    }
}