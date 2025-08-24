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
using DCCPanelController.View.Properties;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class RoutesViewModel : ConnectionViewModel {
    private const string _labelID = "ID";
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
    private ProfileService _profileService;
    
    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;
    private ILogger<RoutesViewModel> _logger;

    public RoutesViewModel(ILogger<RoutesViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        Routes = _profileService?.ActiveProfile?.Routes ?? throw new ArgumentNullException(nameof(profileService),"RoutesViewModel: Active profile is not defined.");
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Routes) ?? false;
        SetLabels();
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
            _                       => RouteStateEnum.Active
        };
        if (!string.IsNullOrEmpty(route.Id) && IsConnected) {
            if (ConnectionService.Client is { } client) await client.SendRouteCmdAsync(route, true)!;
        }
    }
}