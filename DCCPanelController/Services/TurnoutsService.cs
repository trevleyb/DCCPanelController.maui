using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class TurnoutsService {
    
    public ObservableCollection<Turnout> Turnouts = [];

    public TurnoutsService(SettingsService settingsService) {
        Turnouts = settingsService.Turnouts;
    }

    public void AddTurnoutAsync(Turnout turnout) {
        try {
            Turnouts.Add(turnout);
        } catch (Exception ex) {
            Console.WriteLine("Failed to add turnout: " + ex.Message);
        }
    }

    public async Task DeleteTurnoutAsync(string Id) {
        try {
            if (await GetTurnoutByIdAsync(Id) is { } found) {
                Turnouts.Remove(found);
            }
        } catch (Exception ex) {
            Console.WriteLine("Failed to delete turnout: " + ex.Message);
        }
    }
    
    public async Task<Turnout?> GetTurnoutByIdAsync(string id) {
        try {
            return await Task.Run(() => Turnouts.FirstOrDefault(t => t.Id != null && t.Id.Equals(id, StringComparison.OrdinalIgnoreCase)));
        } catch (Exception ex) {
            Console.WriteLine("Failed to find turnout: " + ex.Message);
        }
        return null;
    }
}