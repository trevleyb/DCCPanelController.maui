using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients;
using DCCClients.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
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

    private bool _isAscending;
    [ObservableProperty] private ObservableCollection<Route> _routes;
    private string _sortColumn = "";

    private Profile Profile { get; init; }
    private IDccClient Client { get; init; }
    private ConnectionService? ConnectionService { get; }

    public RoutesViewModel(Profile profile, ConnectionService connectionService) {
        Profile = profile;
        ConnectionService = connectionService;
        Routes = Profile.Routes;
        Client = ConnectionService.GetClient(profile.ActiveConnection);
        Client.RouteMsgReceived += ClientOnRouteMsgReceived;
        SetLabels();
    }

    private void ClientOnRouteMsgReceived(object? sender, DccRouteArgs e) {
        if (Routes.Any(x => x.Id == e.RouteId)) {
            var route = Routes.First(x => x.Id == e.RouteId);
            route.State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive;
        } else {
            Routes.Add(new Route() { Id = e.RouteId, State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive });
        }
    }

    [RelayCommand]
    public async Task SortByColumn(string columnName) {
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

    [RelayCommand(CanExecute = nameof(CanToggleRoutesState))]
    public async Task ToggleRoutesState(Route? route) {
        if (route == null) return;

        route.State = route.State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            RouteStateEnum.Inactive => RouteStateEnum.Active,
            _                       => RouteStateEnum.Active
        };
        if (!string.IsNullOrEmpty(route.Id)) Client?.SendRouteCmd(route.Id, route.State == RouteStateEnum.Active);
    }
}