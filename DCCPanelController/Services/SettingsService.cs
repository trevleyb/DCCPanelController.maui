using System.Text.Json;
using DCCPanelController.Helpers;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class SettingsService {

    private bool _sampleData = false;
    private Storage _storage;

    public SettingsService() {
        //_storage = Load().Result;
        _storage = CreateSampleData().Result;
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

    public async Task<Storage> LoadSampleData() {
        _sampleData = true;
        await using var stream = await FileSystem.OpenAppPackageFileAsync("sampledata.json");
        using var reader = new StreamReader(stream);
        var contents = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<Storage>(contents) ?? new Storage();
    }

    public async Task<Storage> CreateSampleData() {
        _sampleData = true;

        var storage = new Storage();
        storage.Settings = new Settings {
            IpAddress = "192.168.0.1",
            Port = 12090
        };

        storage.Panels = new List<Panel>();
        storage.Panels.Add(new Panel {
            Id = "1",
            Name = "Panel 1",
            ImageAsBase64 = ImageHelper.Base64EncodedImage,
            Turnouts = new List<TurnoutPoint> {
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
            Id = "2",
            Name = "Panel 2",
            ImageAsBase64 = ImageHelper.SampleImageAsBase64,
            Turnouts = new List<TurnoutPoint> {
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
        return storage;
    }

    
    public async void Save() {
        if (!_sampleData) {
            var jsonString = JsonSerializer.Serialize(_storage);
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "storage.json");
            await File.WriteAllTextAsync(filePath, jsonString);
        }
    }

    public Settings Settings => _storage.Settings;
    public List<Panel> Panels => _storage.Panels;
    public Panel? GetPanel(string id) => Panels.FirstOrDefault(p => p.Id == id);
    public void AddPanel(Panel panel) => Panels.Add(panel);
    public List<TurnoutPoint> GetTurnouts(string panelID) => GetPanel(panelID)?.Turnouts ?? [];
    public TurnoutPoint? GetTurnout(string panelID, string turnoutID) => GetTurnouts(panelID).FirstOrDefault(t => t.Id == turnoutID);
    public void AddTurnout(string panelID, TurnoutPoint turnout) => GetTurnouts(panelID).Add(turnout);

    private string SampleImage => ImageHelper.Base64EncodedImage;
}

