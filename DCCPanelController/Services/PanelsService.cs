using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class PanelsService {
    
    public async Task<List<Panel>> GetPanels()
    {
        try {
            var storage = DependencyService.Get<StorageService>();
            if (storage == null) return new List<Panel>();
            return storage.Panels;
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to get Turnout States from JSON File: {ex.Message}");
        }
        return new List<Panel>();
    }
}