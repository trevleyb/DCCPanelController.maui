using System.Runtime.CompilerServices;
using System.Text.Json;
using DCCPanelController.Helpers;

namespace DCCPanelController.Models.DataModel.Repository;

public static class JsonRepository {
    
    public static async Task SaveAsync(Profile profile, string profileName = "default", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        using (new CodeTimer($"Save JSON File: {caller}@{lineNumber}")) {
            try {
                var jsonString = JsonSerializer.Serialize(profile, JsonOptions.Options);
                if (string.IsNullOrEmpty(jsonString)) return;

                var fileName = GetStorageFilePath(profileName);
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

                await File.WriteAllTextAsync(fileName, jsonString);
                Console.WriteLine($"Saved Data: {fileName}");
            } catch (Exception ex) {
                Console.WriteLine($"Unable to SAVE Data: {ex.Message}");
            }
        }
    }
    
    public static async Task<Profile> LoadAsync(string profileName = "default", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) {
        var filePath = GetStorageFilePath(profileName);
        using (new CodeTimer($"Load JSON File: {caller}@{lineNumber}")) {
            try {
                if (File.Exists(filePath)) {
                    try {
                        var jsonString = await File.ReadAllTextAsync(filePath);
                        var profile = JsonSerializer.Deserialize<Profile?>(jsonString, JsonOptions.Options) ?? new Profile(profileName);
                        profile.FixLoadedPanels();
                        return profile;
                    } catch (Exception ex) {
                        Console.WriteLine("Could not deserialize settings. New set created: " + ex.Message);
                        return new Profile(profileName);
                    }
                }
                Console.WriteLine($"File not found: {profileName}");
                return new Profile(profileName);
            } catch (Exception ex) {
                Console.WriteLine("Could not access Profile. New Profile created. " + ex.Message);
                return new Profile(profileName);
            }
        }
    }

    /// <summary>
    ///     Deletes the specified file if it exists.
    /// </summary>
    /// <param name="profileName">The name of the file to be deleted. Defaults to "DCCPanelController.json" if not provided.</param>
    /// <exception cref="Exception">Thrown if an unexpected error occurs while attempting to delete the file.</exception>
    public static async Task Delete(string profileName = "default") {
        try {
            var filePath = GetStorageFilePath(profileName);
            if (File.Exists(filePath)) File.Delete(filePath);
        } catch (Exception ex) {
            Console.WriteLine("Could not delete settings. " + ex.Message);
        }
    }

    /// <summary>
    ///     Serializes the provided Storage instance into a JSON string format.
    /// </summary>
    /// <param name="profile">The Storage object containing the data and settings to be serialized.</param>
    /// <returns>A JSON string representation of the provided Storage object, or an empty string if an error occurs.</returns>
    public static string DownloadSettings(Profile profile) {
        try {
            return JsonSerializer.Serialize(profile, JsonOptions.Options);
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize Profile. Trying to Reload Existing" + ex.Message);
            return string.Empty;
        }
    }

    public static string DownloadProfile(Profile profile) {
        try {
            return JsonSerializer.Serialize(profile, JsonOptions.Options);
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize Profile. Trying to Reload Existing" + ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    ///     Reads and deserializes the JSON data from the provided file to create and return a Storage object.
    /// </summary>
    public static async Task<Profile> UploadSettingsAsync(string jsonString) {
        try {
            var profile = JsonSerializer.Deserialize<Profile?>(jsonString, JsonOptions.Options) ?? throw new ApplicationException("Could not deserialize settings.");
            profile.FixLoadedPanels();
            return profile;
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize settings. Trying to Reload Existing" + ex.Message);
            return await LoadAsync();
        }
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
    private static string GetStorageFilePath(string profileName = "default") {
        try {
            var storageDir = Path.Combine(FileSystem.AppDataDirectory, "DCCPanelController");
            if (!Directory.Exists(storageDir)) Directory.CreateDirectory(storageDir);
            var storageFile = Path.Combine(storageDir, profileName + ".json");
            Console.WriteLine(storageFile);
            return storageFile;
        } catch (Exception ex) {
            Console.WriteLine("Unable to determine where to store the Config File. " + ex.Message);
            throw;
        }
    }
}