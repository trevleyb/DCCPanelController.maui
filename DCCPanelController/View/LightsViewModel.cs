using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class LightsViewModel : ConnectionViewModel {
    private const string _labelID = "ID";
    private const string _labelName = "Light";
    private const string _labelState = "Lit?";
    private readonly ProfileService _profileService;
    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;
    private bool _isAscending;

    [ObservableProperty] private ObservableCollection<Light> _lights;
    private ILogger<LightsViewModel> _logger;
    private string _sortColumn = "";

    public LightsViewModel(ILogger<LightsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (sender, args) => {
            Lights = _profileService?.ActiveProfile?.Lights ?? throw new ArgumentNullException(nameof(profileService), "LightsViewModel: Active profile is not defined.");
            IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Lights) ?? false;
            SetLabels();
        };
        Lights = _profileService?.ActiveProfile?.Lights ?? throw new ArgumentNullException(nameof(profileService), "LightsViewModel: Active profile is not defined.");
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Lights) ?? false;
        SetLabels();
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Light> sorted;
        if (!_isAscending) {
            sorted = columnName switch {
                _labelName  => Lights.OrderBy<Light, string>(x => x.Name ?? "").ToList(),
                _labelID    => Lights.OrderBy<Light, string>(x => x.Id ?? "").ToList(),
                _labelState => Lights.OrderBy<Light, bool>(x => x.State).ToList(),
                _           => Lights.ToList<Light>()
            };
        } else {
            sorted = columnName switch {
                _labelName  => Lights.OrderByDescending<Light, string>(x => x.Name ?? "").ToList(),
                _labelID    => Lights.OrderByDescending<Light, string>(x => x.Id ?? "").ToList(),
                _labelState => Lights.OrderByDescending<Light, bool>(x => x.State).ToList(),
                _           => Lights.ToList<Light>()
            };
        }

        Lights = new ObservableCollection<Light>(sorted);
        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(Lights));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals(_labelID) ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals(_labelName) ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals(_labelState) ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    public async Task ToggleLightState(Light? light) {
        if (light == null) return;
        light.State = !light.State;
        if (!string.IsNullOrEmpty(light.Id) && IsConnected) {
            if (ConnectionService.Client is { } client) await client.SendLightCmdAsync(light, light.State)!;
        }
    }

    [RelayCommand]
    private async Task RefreshLightsAsync() {
        IsBusy = true;
        try {
            if (_profileService?.ActiveProfile is { } profile) profile.RefreshLights();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
            OnPropertyChanged(nameof(Lights));
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}