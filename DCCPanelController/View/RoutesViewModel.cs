using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using DCCPanelController.View.Properties;

namespace DCCPanelController.View;

public partial class RoutesViewModel : ConnectionViewModel {
    private const string _labelID = "System Name";
    private const string _labelName = "User Name";
    private const string _labelState = "State";
    private const string _labelAddress = "DCC Address";

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;
    public string LabelAddress => _labelAddress;

    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;
    [ObservableProperty] private ObservableCollection<Route> _routes;

    private string _sortColumn = "";
    private bool _isAscending;

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    public RoutesViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        ArgumentNullException.ThrowIfNull(Profile);
        Routes = Profile.Routes;
        IsSupported = profile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapabilities.Routes) ?? false;
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
            sortedRoutes = columnName switch {
                _labelName  => Routes.OrderBy<Route, string>(x => x.Name ?? "").ToList(),
                _labelID    => Routes.OrderBy<Route, string>(x => x.Id ?? "").ToList(),
                _labelState => Routes.OrderBy<Route, RouteStateEnum>(x => x.State).ToList(),
                _           => Routes.ToList<Route>()
            };
        } else {
            sortedRoutes = columnName.ToLower() switch {
                _labelName  => Routes.OrderByDescending<Route, string>(x => x.Name ?? "").ToList(),
                _labelID    => Routes.OrderByDescending<Route, string>(x => x.Id ?? "").ToList(),
                _labelState => Routes.OrderByDescending<Route, RouteStateEnum>(x => x.State).ToList(),
                _           => Routes.ToList<Route>()
            };
        }

        Routes = new ObservableCollection<Route>(sortedRoutes);

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(Routes));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals(_labelID) ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals(_labelName) ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals(_labelState) ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    private async Task RefreshRoutesAsync() {
        IsBusy = true;
        try {
            for (var ptr = Profile.Routes.Count; ptr > 0; ptr--) {
                Profile.Routes.RemoveAt(ptr - 1);
                OnPropertyChanged(nameof(Routes));
            }
            await RefreshRoutesAsync();
        } catch { /* ignored */
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
            await ConnectionService.SendRouteCmdAsync(route, true)!;
        }
    }
}