using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class RoutesViewModel : BaseViewModel {
    [ObservableProperty] private ObservableCollection<Route>? _routes;

    private ConnectionService? ConnectionService { get; }
    //private readonly RoutesService? _routesService;
    private bool _isAscending = false;
    private string _sortColumn = "";

    public RoutesViewModel(RoutesService? routesService, ConnectionService? connectionService) {
        ConnectionService = connectionService;
        Routes = routesService?.Routes ?? [];
        CanToggleRoutesState = ConnectionService is not null && ConnectionService.IsConnected;
        SetLabels();
    }

    [ObservableProperty] private bool _canToggleRoutesState;
    [ObservableProperty] private string _columnLabelID = "ID";
    [ObservableProperty] private string _columnLabelName = "Route SystemName";
    [ObservableProperty] private string _columnLabelState = "State";

    [RelayCommand]
    public async Task SortByColumn(string columnName) {
        List<Route> sortedRoutes;
        if (Routes != null) {
            if (!_isAscending) {
                sortedRoutes = columnName.ToLower() switch {
                    "name"  => Routes.OrderBy(x => x.Name).ToList(),
                    "id"    => Routes.OrderBy(x => x.Id).ToList(),
                    "state" => Routes.OrderBy(x => x.State).ToList(),
                    _       => Routes.ToList()
                };
            } else {
                sortedRoutes = columnName.ToLower() switch {
                    "name"  => Routes.OrderByDescending(x => x.Name).ToList(),
                    "id"    => Routes.OrderByDescending(x => x.Id).ToList(),
                    "state" => Routes.OrderByDescending(x => x.State).ToList(),
                    _       => Routes.ToList()
                };
            }

            Routes = new ObservableCollection<Route>(sortedRoutes);
        }

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(Routes));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = "ID" + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = "Route SystemName" + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = "State" + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand(CanExecute = nameof(CanToggleRoutesState))]
    public async Task ToggleRoutesState(Route? route) {
        if (route == null) return;
        route.State = route.State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            RouteStateEnum.Inactive => RouteStateEnum.Active,
            _                       => RouteStateEnum.Active
        };

        if (!string.IsNullOrEmpty(route.Id)) ConnectionService?.SendRouteStateChangeCommand(route.Id, route.State);
    }
}