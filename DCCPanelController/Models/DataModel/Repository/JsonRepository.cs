using System.Text.Json;
using DCCPanelController.Helpers;

namespace DCCPanelController.Models.DataModel.Repository;

public static class JsonRepository {
    #region Save Storage Object to JSON file
    /// <summary>
    ///     Serializes the provided Storage object to a JSON string and saves it to the specified file.
    ///     Uses default JSON serialization options and handles exceptions during the saving process.
    /// </summary>
    /// <param name="profile">The Storage object containing data to serialize and save. Cannot be null.</param>
    /// <param name="profileName">
    ///     The name of the file where the serialized JSON data will be saved.
    ///     Defaults to "DCCPanelController.json" if not specified.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when the provided storage is null.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during the serialization or saving process.</exception>
    public static async Task SaveAsync(Profile profile, string profileName = "default") {
        using (new CodeTimer("Save JSON File")) {
            try {
                var jsonString = JsonSerializer.Serialize(profile, JsonOptions.Options);
                await SaveToFileAsync(GetStorageFilePath(profileName), jsonString);
            } catch (Exception ex) {
                Console.WriteLine($"Unable to SAVE Data: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     Writes the provided JSON string representation of data to the specified file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the JSON data to. Cannot be null, empty, or whitespace.</param>
    /// <param name="jsonString">The JSON string content to write to the file. Cannot be null or empty.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided file name is null, empty, or whitespace.</exception>
    private static async Task SaveToFileAsync(string fileName, string jsonString) {
        if (string.IsNullOrEmpty(jsonString)) return;
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        Console.WriteLine($"Saving to {fileName}");
        await File.WriteAllTextAsync(fileName, jsonString);
    }
    #endregion

    #region Load Storage Object from JSON file
    /// <summary>
    ///     Reads and loads the storage data from the specified file.
    ///     If the file exists, it reads the content, deserializes it into a Storage object, and ensures all panels are
    ///     properly referenced.
    ///     If the file does not exist or an error occurs, a new empty Storage object is returned.
    /// </summary>
    /// <param name="profileName">
    ///     The name of the file to load the storage data from. Defaults to "DCCPanelController.json" if
    ///     not provided.
    /// </param>
    /// <returns>
    ///     A Storage object containing the loaded data, or an empty Storage object if the file does not exist or an error
    ///     occurs.
    /// </returns>
    /// <exception cref="Exception">Thrown if an unexpected error occurs while attempting to read or process the file.</exception>
    public static Profile Load(string profileName = "default") {
        var others = Profiles;
        var filePath = GetStorageFilePath(profileName);
        using (new CodeTimer("Load JSON File")) {
            try {
                if (File.Exists(filePath)) {
                    try {
                        var jsonString = File.ReadAllText(filePath);
                        var profile = LoadFromJson(jsonString, profileName);
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
    ///     Deserializes the provided JSON string into a Storage object.
    /// </summary>
    /// <param name="jsonString">The JSON string containing the serialized Storage data. Cannot be null or empty.</param>
    /// <returns>
    ///     A Storage object deserialized from the JSON string. If deserialization fails or the JSON is invalid, a new
    ///     Storage object is returned.
    /// </returns>
    /// <exception cref="JsonException">Thrown when the JSON string cannot be deserialized into a Storage object.</exception>
    private static Profile LoadFromJson(string jsonString, string profileName = "default") {
        try {
            var result = JsonSerializer.Deserialize<Profile?>(jsonString, JsonOptions.Options);
            return result ?? new Profile(profileName);
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize profile. New profile created: " + ex.Message);
            throw;
        }
    }
    #endregion

    #region Supporting Methods
    /// <summary>
    ///     Deletes the specified file if it exists.
    /// </summary>
    /// <param name="profileName">The name of the file to be deleted. Defaults to "DCCPanelController.json" if not provided.</param>
    /// <exception cref="Exception">Thrown if an unexpected error occurs while attempting to delete the file.</exception>
    public static void Delete(string profileName = "default") {
        try {
            var filePath = GetStorageFilePath(profileName);
            if (File.Exists(filePath)) File.Delete(filePath);
        } catch (Exception ex) {
            Console.WriteLine("Could not delete settings. " + ex.Message);
        }
    }

    /// <summary>
    ///     Retrieves a list of profile names available in the application's data directory.
    ///     The profiles are determined by the files present in the directory used for storing settings.
    ///     The list is generated from the file names without their extensions.
    ///     Ensures that the storage directory exists before attempting to retrieve profiles.
    /// </summary>
    /// <remarks>
    ///     If the directory does not exist, it is created automatically.
    ///     Returns an empty list if no files are found in the directory.
    /// </remarks>
    public static List<string> Profiles {
        get {
            var storageDir = Path.Combine(FileSystem.AppDataDirectory, "DCCPanelController");
            if (!Directory.Exists(storageDir)) Directory.CreateDirectory(storageDir);
            var storageFiles = Directory.GetFiles(storageDir);
            return storageFiles.Select(Path.GetFileNameWithoutExtension).ToList()!;
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
    public static Profile UploadSettings(string jsonString) {
        try {
            return LoadFromJson(jsonString);
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize settings. Trying to Reload Existing" + ex.Message);
            return Load();
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
    #endregion
}