using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class RoutesViewModel : BaseViewModel {
    private const string LabelID = "ID";
    private const string LabelName = "Route";
    private const string LabelState = "State";

    [ObservableProperty] private bool _canToggleRoutesState;
    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;
    [ObservableProperty] private ObservableCollection<Route> _routes;

    private bool _isAscending;
    private string _sortColumn = "";

    public RoutesViewModel(RoutesService? routesService, ConnectionService? connectionService) {
        ConnectionService = connectionService;
        Routes = routesService?.Routes ?? [];
        CanToggleRoutesState = ConnectionService is not null && ConnectionService.IsConnected;
        SetLabels();
    }

    private ConnectionService? ConnectionService { get; }

    [RelayCommand]
    public async Task SortByColumn(string columnName) {
        List<Route> sortedRoutes;
        if (!_isAscending) {
            sortedRoutes = columnName.ToLower() switch {
                "name"  => Enumerable.OrderBy<Route, string>(Routes, x => x.Name).ToList(),
                "id"    => Enumerable.OrderBy<Route, string>(Routes, x => x.Id).ToList(),
                "state" => Enumerable.OrderBy<Route, RouteStateEnum>(Routes, x => x.State).ToList(),
                _       => Enumerable.ToList<Route>(Routes)
            };
        } else {
            sortedRoutes = columnName.ToLower() switch {
                "name"  => Enumerable.OrderByDescending<Route, string>(Routes, x => x.Name).ToList(),
                "id"    => Enumerable.OrderByDescending<Route, string>(Routes, x => x.Id).ToList(),
                "state" => Enumerable.OrderByDescending<Route, RouteStateEnum>(Routes, x => x.State).ToList(),
                _       => Enumerable.ToList<Route>(Routes)
            };
        }

        Routes = new ObservableCollection<Route>(sortedRoutes);

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(ViewModel.RoutesViewModel.Routes));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand(CanExecute = nameof(ViewModel.RoutesViewModel.CanToggleRoutesState))]
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