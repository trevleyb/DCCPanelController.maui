using System.Collections.ObjectModel;
using System.Text.Json;
using DCCPanelController.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DCCPanelController.Services;

public class SettingsService {
    private const string StorageFilename = "DCCPanelController.json";
    private readonly Storage _storage;
    private readonly JsonSerializerOptions? _options = new() { WriteIndented = true };
    
    public SettingsService() {
        _storage = Load();
        _storage.ReOrderPanels();
    }

    public Settings Settings => _storage.Settings;
    public ObservableCollection<Panel> Panels => _storage.Panels;
    public ObservableCollection<Turnout> Turnouts => _storage.Turnouts;
    public ObservableCollection<Route> Routes => _storage.Routes;

    public string ToJsonString() => JsonSerializer.Serialize(_storage, _options);
    public Storage? FromJsonString(string jsonString) => JsonSerializer.Deserialize<Storage?>(jsonString, _options);

    public void Save(string fileName = StorageFilename) {
        try {
            var jsonString = ToJsonString();
            SaveJson(fileName, jsonString);
        } catch (Exception ex) {
            Console.WriteLine($"Unable to SAVE Data: {ex.Message}");
        }
    }

    private void SaveJson(string fileName, string jsonString) {
        File.WriteAllText(GetStorageFilePath(fileName), jsonString);
    }

    public Storage Load(string fileName = StorageFilename) {
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
        try {
            var result = FromJsonString(jsonString);
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
        var storageFile = Path.Combine(FileSystem.AppDataDirectory,"DCCPanelController",filename);
        Console.WriteLine(storageFile);
        return storageFile;
    }
}