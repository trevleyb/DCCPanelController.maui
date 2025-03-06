using System.Text.Json;
using DCCPanelController.Model.DataModel.Helpers;

namespace DCCPanelController.Model.DataModel.Repository;

public static class JsonRepository {
    private const string StorageFilename = "DCCPanelController.json";
    
    #region Save Storage Object to JSON file
    /// <summary>
    /// Serializes the provided Storage object to a JSON string and saves it to the specified file.
    /// Uses default JSON serialization options and handles exceptions during the saving process.
    /// </summary>
    /// <param name="storage">The Storage object containing data to serialize and save. Cannot be null.</param>
    /// <param name="fileName">The name of the file where the serialized JSON data will be saved. Defaults to "DCCPanelController.json" if not specified.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided storage is null.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during the serialization or saving process.</exception>
    public static void Save(Storage storage, string fileName = StorageFilename) {
        try {
            var jsonString = JsonSerializer.Serialize(storage, JsonOptions.Options);
            SaveToFile(fileName, jsonString);
        } catch (Exception ex) {
            Console.WriteLine($"Unable to SAVE Data: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes the provided JSON string representation of data to the specified file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the JSON data to. Cannot be null, empty, or whitespace.</param>
    /// <param name="jsonString">The JSON string content to write to the file. Cannot be null or empty.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided file name is null, empty, or whitespace.</exception>
    private static void SaveToFile(string fileName, string jsonString) {
        if (string.IsNullOrEmpty(jsonString)) return;
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        Console.WriteLine($"Saving to {fileName}");
        File.WriteAllText(GetStorageFilePath(fileName), jsonString);
    }
    #endregion
    
    #region Load Storage Object from JSON file
    /// <summary>
    /// Reads and loads the storage data from the specified file.
    /// If the file exists, it reads the content, deserializes it into a Storage object, and ensures all panels are properly referenced.
    /// If the file does not exist or an error occurs, a new empty Storage object is returned.
    /// </summary>
    /// <param name="fileName">The name of the file to load the storage data from. Defaults to "DCCPanelController.json" if not provided.</param>
    /// <returns>A Storage object containing the loaded data, or an empty Storage object if the file does not exist or an error occurs.</returns>
    /// <exception cref="Exception">Thrown if an unexpected error occurs while attempting to read or process the file.</exception>
    public static Storage Load(string fileName = StorageFilename) {
        var filePath = GetStorageFilePath(fileName);
        try {
            if (File.Exists(filePath)) {
                try {
                    var jsonString = File.ReadAllText(filePath);
                    var storage = LoadFromJson(jsonString);

                    // Each panel needs a reference to the collection
                    // of ALL panels. When we load from storage the panels
                    // we need to reset this reference. Creating a new panel
                    // needs to be done via the storage service which will set this. 
                    foreach (var panel in storage.Panels) panel.Panels = storage.Panels;
                    return storage;
                } catch (Exception ex) {
                    Console.WriteLine("Could not deserialize settings. New set created: " + ex.Message);
                    return new Storage();
                }
            }
            Console.WriteLine($"File not found: {filePath}");
            return new Storage();
        } catch (Exception ex) {
            Console.WriteLine("Could not access settings. New set created: " + ex.Message);
            return new Storage();
        }
    }

    /// <summary>
    /// Deserializes the provided JSON string into a Storage object.
    /// </summary>
    /// <param name="jsonString">The JSON string containing the serialized Storage data. Cannot be null or empty.</param>
    /// <returns>A Storage object deserialized from the JSON string. If deserialization fails or the JSON is invalid, a new Storage object is returned.</returns>
    /// <exception cref="JsonException">Thrown when the JSON string cannot be deserialized into a Storage object.</exception>
    private static Storage LoadFromJson(string jsonString) {
        try {
            var result = JsonSerializer.Deserialize<Storage?>(jsonString, JsonOptions.Options);
            return result ?? new Storage();
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize settings. New set created: " + ex.Message);
            throw;
        }
    }
    #endregion
    
    #region Supporting Methods
    /// <summary>
    /// Deletes the specified file, if it exists.
    /// </summary>
    /// <param name="fileName">The name of the file to be deleted. Defaults to "DCCPanelController.json" if not provided.</param>
    /// <exception cref="Exception">Thrown if an unexpected error occurs while attempting to delete the file.</exception>
    public static void Delete(string fileName = StorageFilename) {
        try {
            var filePath = GetStorageFilePath(fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
        } catch (Exception ex) {
            Console.WriteLine("Could not delete settings. " + ex.Message);
        }
    }

    /// <summary>
    /// Serializes the provided Storage instance into a JSON string format.
    /// </summary>
    /// <param name="storage">The Storage object containing the data and settings to be serialized.</param>
    /// <returns>A JSON string representation of the provided Storage object, or an empty string if an error occurs.</returns>
    public static string DownloadSettings(Storage storage) {
        try {
            return JsonSerializer.Serialize(storage, JsonOptions.Options);
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize settings. Trying to Reload Existing" + ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// Reads and deserializes the JSON data from the provided file to create and return a Storage object.
    /// </summary>
    /// <param name="settingsFile">The path of the JSON file containing the settings data to be uploaded.</param>
    /// <returns>A Storage object populated with the data from the JSON file, or a new Storage object if deserialization fails.</returns>
    public static Storage UploadSettings(string settingsFile) {
        try {
            return LoadFromJson(settingsFile);
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize settings. Trying to Reload Existing" + ex.Message);
            return Load();
        }
    }

    /// <summary>
    /// Constructs the full file path for the specified file name by combining it with the
    /// application data directory and ensuring the storage directory exists.
    /// </summary>
    /// <param name="filename">The name of the file for which the full storage path is to be determined.</param>
    /// <returns>The full file path within the application's configured storage directory.</returns>
    private static string GetStorageFilePath(string filename) {
        try {
            var storageDir = Path.Combine(FileSystem.AppDataDirectory, "DCCPanelController");
            if (!Directory.Exists(storageDir)) Directory.CreateDirectory(storageDir);
            var storageFile = Path.Combine(storageDir, filename);
            Console.WriteLine(storageFile);
            return storageFile;
        } catch (Exception ex) {
            Console.WriteLine("Unable to determine where to store the Config File. " + ex.Message);
            throw;
        }
    }
    #endregion
}