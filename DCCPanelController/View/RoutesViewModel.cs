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
using Route = DCCPanelController.Models.DataModel.Accessories.Route;

namespace DCCPanelController.View;

public partial class RoutesViewModel : AccessoryViewModel<Route>
{
    private const string _labelID    = "ID";
    private const string _labelName  = "User Name";
    private const string _labelState = "State";

    private readonly ILogger<RoutesViewModel> _logger;

    [ObservableProperty] private string _columnLabelID    = _labelID;
    [ObservableProperty] private string _columnLabelName  = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;

    public RoutesViewModel(ILogger<RoutesViewModel> logger, ProfileService profileService, ConnectionService connectionService)
        : base(profileService, connectionService)
    {
        _logger = logger;
    }

    public ObservableCollection<Route> Routes
    {
        get => Items;
        private set => Items = value;
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    public void SetToolbarItems()
    {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Routes) ?? false;
        _ = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }

    protected override string DefaultSortKey => _labelName;

    protected override ObservableCollection<Route> ResolveCollection(Profile profile) => profile.Routes;

    protected override IReadOnlyDictionary<string, Func<Route, IComparable>> Sorters => new Dictionary<string, Func<Route, IComparable>>
    {
        [_labelName]  = x => x.Name ?? "",
        [_labelID]    = x => x.Id ?? "",
        [_labelState] = x => x.State
    };

    protected override void UpdateColumnLabels()
    {
        ColumnLabelID = LabelWithArrow(_labelID, _labelID);
        ColumnLabelName = LabelWithArrow(_labelName, _labelName);
        ColumnLabelState = LabelWithArrow(_labelState, _labelState);
    }

    protected override void OnItemsRebound() => OnPropertyChanged(nameof(Routes));

    [RelayCommand]
    private async Task RefreshRoutesAsync()
    {
        IsBusy = true;
        try
        {
            _profileService.ActiveProfile?.RefreshRoutes();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task ToggleRoutesState(Route? route)
    {
        if (route == null) return;
        route.State = route.State switch
        {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            RouteStateEnum.Inactive => RouteStateEnum.Active,
            _                       => RouteStateEnum.Active,
        };
        if (!string.IsNullOrEmpty(route.Id))
        {
            if (ConnectionService.Client is { State: DccClientState.Connected } client)
                await client.SendRouteCmdAsync(route, true)!;
        }
    }
}
