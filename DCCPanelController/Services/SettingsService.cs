using System.Collections.ObjectModel;
using System.Text.Json;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DCCPanelController.Services;

public class SettingsService {
    private const string StorageFilename = "DCCPanelController.json";
    private Storage? _storage;
    private readonly JsonSerializerOptions? _options = new() {
        WriteIndented = true, 
        Converters = {
            new JsonConverterTrackPiece(),
            new MauiColorJsonConverter()
        }
    };
    
    public Storage Storage => _storage ??= Load();
    public Settings Settings => Storage.Settings;
    public ObservableCollection<Panel> Panels => Storage.Panels;
    public ObservableCollection<Turnout> Turnouts => Storage.Turnouts;
    public ObservableCollection<Route> Routes => Storage.Routes;
    
    public void Save(string fileName = StorageFilename) {
        try {
            var jsonString = JsonSerializer.Serialize(_storage, _options);
            SaveJson(fileName, jsonString);
        } catch (Exception ex) {
            Console.WriteLine($"Unable to SAVE Data: {ex.Message}");
        }
    }

    private void SaveJson(string fileName, string jsonString) {
        File.WriteAllText(GetStorageFilePath(fileName), jsonString);
    }

    public Storage Load(string fileName = StorageFilename) {
        Console.WriteLine("Storage Load Called");
        var filePath = GetStorageFilePath(fileName);
        try {
            if (File.Exists(filePath)) {
                try {
                    var jsonString = File.ReadAllText(filePath);
                    return LoadJson(jsonString);
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

    private Storage LoadJson(string jsonString) {
        Console.WriteLine("Storage LoadJson Called");
        try {
            var result = JsonSerializer.Deserialize<Storage?>(jsonString, _options);
            return result ?? new Storage();
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize settings. New set created: " + ex.Message);
            throw;
        }
    }

    public void Delete(string fileName = StorageFilename) {
        var filePath = GetStorageFilePath(fileName);
        if (File.Exists(filePath)) {
            File.Delete(filePath);
        }
    }

    public void UploadNewSettings(string settingsLoaded) {
        try {
            LoadJson(settingsLoaded);
        } catch (Exception ex) {
            Console.WriteLine("Could not deserialize settings. Trying to Reload Existing" + ex.Message);
            Load();
        }
    }
    
    private static string GetStorageFilePath(string filename) {
        try {
            var storageDir = Path.Combine(FileSystem.AppDataDirectory, "DCCPanelController");
            if (!Directory.Exists(storageDir)) Directory.CreateDirectory(storageDir);
            var storageFile = Path.Combine(storageDir, filename);
            Console.WriteLine(storageFile);
            return storageFile;
        } catch (Exception ex) {
            Console.WriteLine("Unable to determine where to store the Config File. "+ ex.Message);
            throw;
        }
    }

    public string ToJsonString() {
        return JsonSerializer.Serialize(_storage, _options);
    }
}