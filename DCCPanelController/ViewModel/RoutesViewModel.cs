using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class RoutesViewModel : BaseViewModel {
    
    public ObservableCollection<Route>? Routes { get; set; } = [];
    private readonly RoutesService? _routesService;
    private bool _isAscending = false;
    private string _sortColumn = "";
    
    public RoutesViewModel(RoutesService? routesService) {
        _routesService = routesService;
        Routes = _routesService?.Routes ?? new ObservableCollection<Route>();
        SetLabels();
    }
    
    [ObservableProperty] private string columnLabelID       = "ID";
    [ObservableProperty] private string columnLabelName     = "Route Name";
    [ObservableProperty] private string columnLabelState    = "State";
    
    [RelayCommand]
    public async Task SortByColumn(string columnName) {

        List<Route> sortedRoutes;
        if (Routes != null) {
            if (!_isAscending) {
                sortedRoutes = columnName.ToLower() switch {
                    "name"  => Routes.OrderBy(x => x.Name).ToList(),
                    "id"    => Routes.OrderBy(x => x.Id).ToList(),
                    "state" => Routes.OrderBy(x => x.State).ToList(),
                    _       => Routes.ToList(),
                };
            } else {
                sortedRoutes = columnName.ToLower() switch {
                    "name"  => Routes.OrderByDescending(x => x.Name).ToList(),
                    "id"    => Routes.OrderByDescending(x => x.Id).ToList(),
                    "state" => Routes.OrderByDescending(x => x.State).ToList(),
                    _       => Routes.ToList(),
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
        ColumnLabelName = "Route Name" + (_sortColumn.Equals("Name") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = "State" + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }
    
    [RelayCommand]
    public async Task ToggleRoutesState(Route? route) {
        
        if (route == null) return;
        route.State = route.State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            RouteStateEnum.Inactive => RouteStateEnum.Active,
            _                       => RouteStateEnum.Active
        };
        
        var connectionSerice = App.ServiceProvider?.GetService<ConnectionService>();
        if (connectionSerice != null && !string.IsNullOrEmpty(route.Id)) {
            connectionSerice.SendRouteStateChangeCommand(route.Id, route.State);
        }
    }
}