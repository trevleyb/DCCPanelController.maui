using System.Collections.ObjectModel;
using System.Text.Json;
using DCCPanelController.Helpers;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class SettingsService {

    private const string StorageFileame = "DCCPanelController.json";
    private readonly Storage _storage;
    
    public SettingsService() {
        _storage = Load(StorageFileame);
        _storage.ReOrderPanels();
    }
    
    public Settings Settings => _storage.Settings;
    public ObservableCollection<Panel> Panels => _storage.Panels;
    public ObservableCollection<Turnout> Turnouts => _storage.Turnouts;
    public ObservableCollection<Route> Routes => _storage.Routes;

    public void Save() => Save(StorageFileame);
    
    public void Save(string fileName) {
        var jsonString = JsonSerializer.Serialize(_storage);
        try {
            File.WriteAllText(GetStorageFilePath(fileName), jsonString);
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }

    public void Delete() => Delete(StorageFileame);

    public void Delete(string fileName) {
        var filePath = GetStorageFilePath(fileName);
        if (File.Exists(filePath)) {
            File.Delete(filePath);
        }
    }


    public Storage Load() => Load(StorageFileame);

    public Storage Load(string fileName) {
        var filePath = GetStorageFilePath(fileName);
        try {
            if (File.Exists(filePath)) {
                try {
                    var jsonString = File.ReadAllText(filePath);
                    var result = JsonSerializer.Deserialize<Storage>(jsonString);
                    return result ?? new Storage();
                } catch (Exception ex) {
                    return new Storage();
                }
            } 
            return new Storage();
        } catch (Exception ex) {
            return new Storage();
        } 
    }

    protected string GetStorageFilePath(string filename) {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var libraryPath = Path.Combine(documentsPath, "..", "Library");
        return Path.Combine(libraryPath, filename);
    }
}

