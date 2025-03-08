// using System.Collections.ObjectModel;
// using DCCPanelController.Model;
// using Route = DCCPanelController.Model.DataModel.Route;
//
// namespace DCCPanelController.Services;
//
// public class RoutesService {
//     public ObservableCollection<Route> Routes = [];
//
//     public RoutesService(SettingsService settingsService) {
//         Routes = settingsService.Routes;
//     }
//
//     // Think about where we manage changes to the state?
//     public event EventHandler? RouteStateDataChanged;
//
//     public async Task AddRouteAsync(Route route) {
//         try {
//             Routes.Add(route);
//         } catch (Exception ex) {
//             Console.WriteLine("Failed to add route: " + ex.Message);
//         }
//     }
//
//     public async Task DeleteRouteAsync(string Id) {
//         try {
//             if (await GetRouteByIdAsync(Id) is { } found) {
//                 Routes.Remove(found);
//             }
//         } catch (Exception ex) {
//             Console.WriteLine("Failed to delete turnout: " + ex.Message);
//         }
//     }
//
//     public async Task<Route?> GetRouteByIdAsync(string id) {
//         try {
//             return Routes.FirstOrDefault(t => t.Id != null && t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
//         } catch (Exception ex) {
//             Console.WriteLine("Failed to find route: " + ex.Message);
//         }
//
//         return null;
//     }
//
//     protected virtual void OnRouteStateDataChanged() {
//         RouteStateDataChanged?.Invoke(this, EventArgs.Empty);
//     }
// }