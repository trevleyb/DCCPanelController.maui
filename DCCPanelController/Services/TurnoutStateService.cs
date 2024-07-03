using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class TurnoutStateService {
    
    // Think about where we manage changes to the state?
    public event EventHandler? TurnoutStateDataChanged;

    private List<Turnout> _turnoutsList = [];
    public async Task<List<Turnout>> GetTurnoutStates()
    {
      
        if (_turnoutsList?.Count > 0) {
            return _turnoutsList;
        }

        // Offline
        try {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("turnoutstatedata.json");
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var list = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(contents, options);
            if (list != null) {
                foreach (var turnout in list.Select(item => new Turnout {
                             Id = item["Id"]?.ToString(),
                             Name = item["Name"]?.ToString(),
                             State = (TurnoutStateEnum)Enum.Parse(typeof(TurnoutStateEnum), item["State"].ToString() ?? "Unknown")
                         })) {
                    _turnoutsList?.Add(turnout);
                }
            }
        } catch (Exception ex) {
            _turnoutsList = [];
            Debug.WriteLine($"Unable to get Turnout States from JSON File: {ex.Message}");
        }
        return _turnoutsList ?? [];
    }

    protected virtual void OnTurnoutStateDataChanged() {
        TurnoutStateDataChanged?.Invoke(this, EventArgs.Empty);
    }
}