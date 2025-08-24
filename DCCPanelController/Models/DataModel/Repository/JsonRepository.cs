using System.Runtime.CompilerServices;
using System.Text.Json;
using DCCPanelController.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.DataModel.Repository;

public static class JsonRepository {

    private static readonly ILogger Logger = LogHelper.CreateLogger("PanelRepository");
    
    public static async Task SaveAsync(Profile profile, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        using (new CodeTimer($"Save JSON File: {caller}@{lineNumber}")) {
            try {
                var jsonString = JsonSerializer.Serialize(profile, JsonOptions.Options);
                if (string.IsNullOrEmpty(jsonString)) return;

                var fileName = GetStorageFilePath(profile.Filename);
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(profile.Filename));
                await File.WriteAllTextAsync(fileName, jsonString);
                Logger.LogInformation("Saved Data: {fileName}",fileName);
            } catch (Exception ex) {
                Logger.LogError("Unable to SAVE Data: {Message}",ex.Message);
            }
        }
        LoggingLevelHelper.SetLogLevel(profile.Settings.LogLevel);
    }

    public static void Save(Profile profile, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        using (new CodeTimer($"Save JSON File: {caller}@{lineNumber}")) {
            try {
                var jsonString = JsonSerializer.Serialize(profile, JsonOptions.Options);
                if (string.IsNullOrEmpty(jsonString)) return;

                var fileName = GetStorageFilePath(profile.Filename);
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(profile.Filename));
                File.WriteAllText(fileName, jsonString);
                Logger.LogInformation("Saved Data: {fileName}",fileName);
            } catch (Exception ex) {
                Logger.LogError("Unable to SAVE Data: {Message}",ex.Message);
            }
        }
        LoggingLevelHelper.SetLogLevel(profile.Settings.LogLevel);
    }

    public static async Task<Profile> LoadAsync(string profileName, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        var filePath = GetStorageFilePath(profileName);
        using (new CodeTimer($"Load JSON File: {caller}@{lineNumber}")) {
            try {
                if (File.Exists(filePath)) {
                    try {
                        var jsonString = await File.ReadAllTextAsync(filePath);
                        var profile = JsonSerializer.Deserialize<Profile?>(jsonString, JsonOptions.Options) ?? new Profile(profileName);
                        LoggingLevelHelper.SetLogLevel(profile.Settings.LogLevel);
                        profile.FixLoadedPanels();
                        return profile;
                    } catch (Exception ex) {
                        Logger.LogError("Could not deserialize settings. New set created: {Message}",ex.Message);
                        return new Profile(profileName);
                    }
                }
                Logger.LogInformation("File not found: {profileName}",profileName);
                var newProfile = new Profile(profileName);
                LoggingLevelHelper.SetLogLevel(newProfile.Settings.LogLevel);
                return newProfile;
            } catch (Exception ex) {
                Logger.LogWarning("Could not access Profile. New Profile created. {Message}",ex.Message);
                return new Profile(profileName);
            }
        }
    }

    
    public static Profile Load(string profileName, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        var filePath = GetStorageFilePath(profileName);
        using (new CodeTimer($"Load JSON File: {caller}@{lineNumber}")) {
            try {
                if (File.Exists(filePath)) {
                    try {
                        var jsonString = File.ReadAllText(filePath);
                        var profile = JsonSerializer.Deserialize<Profile?>(jsonString, JsonOptions.Options) ?? new Profile(profileName);
                        LoggingLevelHelper.SetLogLevel(profile.Settings.LogLevel);
                        profile.FixLoadedPanels();
                        return profile;
                    } catch (Exception ex) {
                        Logger.LogError("Could not deserialize settings. New set created: {Message}",ex.Message);
                        return new Profile(profileName);
                    }
                }
                Logger.LogInformation("File not found: {profileName}",profileName);
                var newProfile = new Profile(profileName);
                LoggingLevelHelper.SetLogLevel(newProfile.Settings.LogLevel);
                return newProfile;
            } catch (Exception ex) {
                Logger.LogWarning("Could not access Profile. New Profile created. {Message}",ex.Message);
                return new Profile(profileName);
            }
        }
    }

    /// <summary>
    ///     Deletes the specified file if it exists.
    /// </summary>
    public static async Task Delete(Profile profile) {
        try {
            var filePath = GetStorageFilePath(profile.Filename);
            if (File.Exists(filePath)) File.Delete(filePath);
        } catch (Exception ex) {
            Logger.LogWarning("Could not delete settings. {Message}",ex.Message);
        }
    }

    public static string DownloadProfile(Profile profile) {
        try {
            return JsonSerializer.Serialize (profile, JsonOptions.Options);
        } catch (Exception ex) {
            Logger.LogWarning("Could not deserialize Profile. Trying to Reload Existing {Message}",ex.Message);
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
            return profile;
        } catch (Exception ex) {
            Logger.LogWarning("Could not deserialize settings. Trying to Reload Existing {Message}",ex.Message);
            return null;
        }
    }
    private static string GetProfileStorageDir() {
        var storageDir = Path.Combine(FileSystem.AppDataDirectory, "DCCPanelController");
        if (!Directory.Exists(storageDir)) Directory.CreateDirectory(storageDir);
        return storageDir;
    }

    /// <summary>
    ///     Constructs and retrieves the full file path for storing the configuration file
    ///     associated with the provided profile name. Creates the necessary storage
    ///     directory if it does not already exist.
    /// </summary>
    /// <param name="profileName">The name of the profile file. Defaults to "default" if not provided.</param>
    /// <returns>A string representing the absolute file path for the specified profile's storage location.</returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs while determining or creating the storage directory or full file
    ///     path.
    /// </exception>
    public static string GetStorageFilePath(string profileName = "default") {
        try {
            var storageDir = GetProfileStorageDir();
            var storageFile = Path.Combine(storageDir, profileName + ".json");
            return storageFile;
        } catch (Exception ex) {
            Logger.LogCritical("Unable to determine where to store the Config File. {Message}",ex.Message);
            throw;
        }
    }
}