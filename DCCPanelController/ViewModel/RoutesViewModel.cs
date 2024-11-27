using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class RoutesViewModel : BaseViewModel {

    private const string LabelID = "ID";
    private const string LabelName = "Route";
    private const string LabelState = "State";

    private ConnectionService? ConnectionService { get; }
    private bool _isAscending = false;
    private string _sortColumn = "";

    [ObservableProperty] private ObservableCollection<Route>? _routes;

    public RoutesViewModel(RoutesService? routesService, ConnectionService? connectionService) {
        ConnectionService = connectionService;
        Routes = routesService?.Routes ?? [];
        CanToggleRoutesState = ConnectionService is not null && ConnectionService.IsConnected;
        SetLabels();
    }

    [ObservableProperty] private bool _canToggleRoutesState;
    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;

    [RelayCommand]
    public async Task SortByColumn(string columnName) {
        List<Route> sortedRoutes = [];
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
        ColumnLabelID = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
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