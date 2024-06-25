using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class RoutesService {
     
    // Think about where we manage changes to the state?
    public event EventHandler? RouteStateDataChanged;

    private List<Route> _routesList = [];
    public async Task<List<Route>> GetRoutes()
    {
      
        if (_routesList?.Count > 0) {
            return _routesList;
        }

        // Offline
        try {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("routesdata.json");
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var list = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(contents, options);
            if (list != null) {
                foreach (var route in list.Select(item => new Route {
                             Id = item["Id"]?.ToString(),
                             Name = item["Name"]?.ToString(),
                             State = (RouteStateEnum)Enum.Parse(typeof(RouteStateEnum), item["State"].ToString() ?? "Unknown")
                         })) {
                    _routesList?.Add(route);
                }
            }
        } catch (Exception ex) {
            _routesList = [];
            Debug.WriteLine($"Unable to get Routes from JSON File: {ex.Message}");
        }

        return _routesList ?? [];
    }

    protected virtual void OnRouteStateDataChanged() {
        RouteStateDataChanged?.Invoke(this, EventArgs.Empty);
    }
}