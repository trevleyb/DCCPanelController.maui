using System.Collections.ObjectModel;
using System.Text.Json;
using DCCPanelController.Helpers;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class SettingsService {

    private bool _sampleData = false;
    private Storage _storage;

    public SettingsService() {
        _storage = Load().WaitAsync(new CancellationToken()).Result;
        _ = AddSampleData(_storage);
        
        for (var index = 0; index < Panels.Count; index++) {
            Panels[index].SortOrder = index+1;
        }
    }
    
    public Settings Settings => _storage.Settings;
    public ObservableCollection<Panel> Panels => _storage.Panels;
    public ObservableCollection<Turnout> Turnouts => _storage.Turnouts;
    public ObservableCollection<Route> Routes => _storage.Routes;
    private string SampleImage => ImageHelper.Base64EncodedImage;

    public async Task Save() {
        if (!_sampleData) {
            var jsonString = JsonSerializer.Serialize(_storage);
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "storage.json");
            await File.WriteAllTextAsync(filePath, jsonString);
        }
    }
    
    public async Task ReLoad(bool useSampleData = false) {
        _storage = await Load();
        if (useSampleData) await AddSampleData(_storage);
    }
    
    public async Task<Storage> Load() {
        _sampleData = false;
        var filePath = Path.Combine(FileSystem.AppDataDirectory, "DCCPanelController.json");
        if (File.Exists(filePath)) {
            var jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<Storage>(jsonString) ?? new Storage();
        } 
        return new Storage();
    }

    public async Task AddSampleData(Storage storage) {
        storage.Panels.Clear();
        storage.Panels.Add(new Panel {
            Id = "P01",
            SortOrder = 5,
            Name = "Sample Panel #1",
            ImageAsBase64 = ImageHelper.SampleImageAsBase64,
            Turnouts = new ObservableCollection<TurnoutPoint>() {
                new TurnoutPoint {
                    Id = "1",
                    Name = "Turnout 1"
                },
                new TurnoutPoint {
                    Id = "2",
                    Name = "Turnout 2"
                }
            }, 
        });
        storage.Panels.Add(new Panel {
            Id = "P02",
            SortOrder = 2,
            Name = "Sample Panel #2",
            ImageAsBase64 = ImageHelper.SampleImageAsBase64,
            Turnouts = new ObservableCollection<TurnoutPoint>(){
                new TurnoutPoint {
                    Id = "1",
                    Name = "Turnout 1"
                },
                new TurnoutPoint {
                    Id = "2",
                    Name = "Turnout 2"
                }
            }, 
        });

        // Load the Sample Turnouts Data
        try {
            using var stream = await FileSystem.OpenAppPackageFileAsync("turnoutsdata.json");
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            _storage.Turnouts = JsonSerializer.Deserialize<ObservableCollection<Turnout>>(contents) ?? new ObservableCollection<Turnout>();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            _storage.Turnouts = new ObservableCollection<Turnout>();
        }

        // Load the Sample Routes Data
        try {
            using var stream = await FileSystem.OpenAppPackageFileAsync("routesdata.json");
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            _storage.Routes = JsonSerializer.Deserialize<ObservableCollection<Route>>(contents) ?? new ObservableCollection<Route>();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            _storage.Routes = new ObservableCollection<Route>();
        }
        
    }
}

