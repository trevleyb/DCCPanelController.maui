using System.Collections.ObjectModel;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class TurnoutsService(SettingsService settingsService) {
    public readonly ObservableCollection<Turnout> Turnouts = settingsService.Turnouts;

    public void AddTurnoutAsync(Turnout turnout) {
        try {
            Turnouts.Add(turnout);
        } catch (Exception ex) {
            Console.WriteLine("Failed to add turnout: " + ex.Message);
        }
    }

    public async Task DeleteTurnoutAsync(string id) {
        try {
            if (await GetTurnoutByIdAsync(id) is { } found) {
                Turnouts.Remove(found);
            }
        } catch (Exception ex) {
            Console.WriteLine("Failed to delete turnout: " + ex.Message);
        }
    }

    public Turnout? GetTurnoutById(string id) {
        try {
            return Turnouts.FirstOrDefault(t => t.Id != null && t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        } catch (Exception ex) {
            Console.WriteLine("Failed to find turnout: " + ex.Message);
        }

        return null;
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