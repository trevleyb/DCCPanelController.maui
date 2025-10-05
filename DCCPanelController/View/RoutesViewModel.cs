using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class RoutesViewModel : ConnectionViewModel {
    private const    string         _labelID      = "ID";
    private const    string         _labelName    = "User Name";
    private const    string         _labelState   = "State";
    private const    string         _labelAddress = "DCC Address";
    private readonly ProfileService _profileService;

    [ObservableProperty] private string                      _columnLabelID    = _labelID;
    [ObservableProperty] private string                      _columnLabelName  = _labelName;
    [ObservableProperty] private string                      _columnLabelState = _labelState;
    private                      bool                        _isAscending;
    private                      ILogger<RoutesViewModel>    _logger;
    [ObservableProperty] private ObservableCollection<Route> _routes;

    private string _sortColumn = "";

    public RoutesViewModel(ILogger<RoutesViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (sender, args) => {
            Routes = _profileService?.ActiveProfile?.Routes ?? throw new ArgumentNullException(nameof(profileService), "RoutesViewModel: Active profile is not defined.");
            IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Routes) ?? false;
            SetLabels();
        };
        _sortColumn = _labelName;
        _isAscending = true;
        var routes = _profileService?.ActiveProfile?.Routes ?? throw new ArgumentNullException(nameof(profileService), "RoutesViewModel: Active profile is not defined.");
        Routes = new ObservableCollection<Route>(routes.OrderBy<Route, string>(x => x.Name ?? "").ToList());
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Routes) ?? false;
        SetLabels();
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;
    public string LabelAddress => _labelAddress;

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    public void SetToolbarItems() {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Routes) ?? false;
        _ = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Route> sortedRoutes;

        if (!_isAscending) {
            sortedRoutes = columnName switch {
                _labelName  => Routes.OrderBy<Route, string>(x => x.Name ?? "").ToList(),
                _labelID    => Routes.OrderBy<Route, string>(x => x.Id ?? "").ToList(),
                _labelState => Routes.OrderBy<Route, RouteStateEnum>(x => x.State).ToList(),
                _           => Routes.ToList<Route>(),
            };
        } else {
            sortedRoutes = columnName.ToLower() switch {
                _labelName  => Routes.OrderByDescending<Route, string>(x => x.Name ?? "").ToList(),
                _labelID    => Routes.OrderByDescending<Route, string>(x => x.Id ?? "").ToList(),
                _labelState => Routes.OrderByDescending<Route, RouteStateEnum>(x => x.State).ToList(),
                _           => Routes.ToList<Route>(),
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
            if (_profileService?.ActiveProfile is { } profile) profile.RefreshRoutes();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
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
            _                       => RouteStateEnum.Active,
        };
        if (!string.IsNullOrEmpty(route.Id)) {
            if (ConnectionService.Client is {State: DccClientState.Connected } client) await client.SendRouteCmdAsync(route, true)!;
        }
    }
}