using System.Collections.ObjectModel;
using System.Text.Json;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class SettingsService {
    private const string StorageFileame = "DCCPanelController.json";
    private readonly Storage _storage;

    private readonly JsonSerializerOptions? options = new() {
        WriteIndented = true
    };

    public SettingsService() {
        _storage = Load(StorageFileame);
        _storage.ReOrderPanels();
    }

    public Settings Settings => _storage.Settings;
    public ObservableCollection<Panel> Panels => _storage.Panels;
    public ObservableCollection<Turnout> Turnouts => _storage.Turnouts;
    public ObservableCollection<Route> Routes => _storage.Routes;

    public string ToJsonString() {
        return JsonSerializer.Serialize(_storage, options);
    }

    public Settings? FromJsonString(string jsonString) {
        return JsonSerializer.Deserialize<Settings>(jsonString, options);
    }

    public void Save() {
        Save(StorageFileame);
    }

    public void Save(string fileName) {
        var jsonString = JsonSerializer.Serialize(_storage, options);
        try {
            File.WriteAllText(GetStorageFilePath(fileName), jsonString);
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }

    public Storage Load() {
        return Load(StorageFileame);
    }

    public Storage Load(string fileName) {
        var filePath = GetStorageFilePath(fileName);
        try {
            if (File.Exists(filePath)) {
                try {
                    var jsonString = File.ReadAllText(filePath);
                    var result = JsonSerializer.Deserialize<Storage>(jsonString, options);
                    return result ?? new Storage();
                } catch (Exception ex) {
                    Console.WriteLine("Could not deserialize settings. New set created: " + ex.Message);
                    return new Storage();
                }
            }

            return new Storage();
        } catch (Exception ex) {
            Console.WriteLine("Could not access settings. New set created: " + ex.Message);
            return new Storage();
        }
    }

    public void Delete() {
        Delete(StorageFileame);
    }

    public void Delete(string fileName) {
        var filePath = GetStorageFilePath(fileName);
        if (File.Exists(filePath)) {
            File.Delete(filePath);
        }
    }

    private static string GetStorageFilePath(string filename) {
        var storageFile = Path.Combine(FileSystem.AppDataDirectory, filename);
        Console.WriteLine(storageFile);
        return storageFile;
    }
}