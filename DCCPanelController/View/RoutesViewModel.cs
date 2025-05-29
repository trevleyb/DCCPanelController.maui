using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using RouteStateEnum = DCCPanelController.Models.DataModel.Entities.RouteStateEnum;

namespace DCCPanelController.View;

public partial class RoutesViewModel : ConnectionViewModel {
    private const string LabelID = "ID";
    private const string LabelName = "Route";
    private const string LabelState = "State";

    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;
    private bool _isAscending;
    [ObservableProperty] private ObservableCollection<Route> _routes;
    private string _sortColumn = "";

    public RoutesViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        ArgumentNullException.ThrowIfNull(Profile);
        Routes = Profile.Routes;
        SetLabels();
    }

    private void ClientOnRouteMsgReceived(object? sender, DccRouteArgs e) {
        if (Routes.Any(x => x.Id == e.RouteId)) {
            var route = Routes.First(x => x.Id == e.RouteId);
            route.State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive;
        } else {
            Routes.Add(new Route { Id = e.RouteId, State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive });
        }
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Route> sortedRoutes;

        if (!_isAscending) {
            sortedRoutes = columnName.ToLower() switch {
                "name"  => Routes.OrderBy<Route, string>(x => x.Name ?? "").ToList(),
                "id"    => Routes.OrderBy<Route, string>(x => x.Id ?? "").ToList(),
                "state" => Routes.OrderBy<Route, RouteStateEnum>(x => x.State).ToList(),
                _       => Routes.ToList<Route>()
            };
        } else {
            sortedRoutes = columnName.ToLower() switch {
                "name"  => Routes.OrderByDescending<Route, string>(x => x.Name ?? "").ToList(),
                "id"    => Routes.OrderByDescending<Route, string>(x => x.Id ?? "").ToList(),
                "state" => Routes.OrderByDescending<Route, RouteStateEnum>(x => x.State).ToList(),
                _       => Routes.ToList<Route>()
            };
        }

        Routes = new ObservableCollection<Route>(sortedRoutes);

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(Routes));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    private async Task RefreshRoutesAsync() {
        try {
            IsBusy = true;
            ConnectionService.ForceRefresh();
        } catch (Exception ex) {
            Console.WriteLine($"Unable to force refresh the routes: {ex.Message}");
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ToggleRoutesState(Route? route) {
        if (route == null) return;
        route.State = route.State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            RouteStateEnum.Inactive => RouteStateEnum.Active,
            _                       => RouteStateEnum.Active
        };
        if (!string.IsNullOrEmpty(route.Id) && IsConnected) {
            await ConnectionService.SendRouteCmdAsync(route.Id, route.State == RouteStateEnum.Active)!;
        }
    }

    [RelayCommand]
    private async Task ClearAllAsync() {
        IsBusy = true;
        try {
            if (await AskUserToConfirm("Reset all Routes?", "This wll remove all Routes previously loaded from a Server and reload them from the Connected Server. Are you sure you want to do this?")) {
                for (var ptr = Profile.Routes.Count; ptr > 0; ptr--) {
                    Profile.Routes.RemoveAt(ptr - 1);
                    OnPropertyChanged(nameof(Routes));
                }
                await RefreshRoutesAsync();
            }
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}