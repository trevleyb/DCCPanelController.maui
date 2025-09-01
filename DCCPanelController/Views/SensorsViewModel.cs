using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.Views.Base;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Views;

public partial class SensorsViewModel : ConnectionViewModel {
    private const string _labelID = "ID";
    private const string _labelName = "Sensor";
    private const string _labelState = "Occupied?";
    private readonly ProfileService _profileService;
    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;

    private bool _isAscending;

    private ILogger<SensorsViewModel> _logger;

    [ObservableProperty] private ObservableCollection<Sensor> _sensors;
    private string _sortColumn = "";

    public SensorsViewModel(ILogger<SensorsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (sender, args) => {
            Sensors = _profileService?.ActiveProfile?.Sensors ?? throw new ArgumentNullException(nameof(profileService), "SensorsViewModel: Active profile is not defined.");
            IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Sensors) ?? false;
            SetLabels();
        };

        Sensors = _profileService?.ActiveProfile?.Sensors ?? throw new ArgumentNullException(nameof(profileService), "SensorsViewModel: Active profile is not defined.");
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Sensors) ?? false;
        SetLabels();
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Sensor> sorted;
        if (!_isAscending) {
            sorted = columnName switch {
                _labelName  => Enumerable.OrderBy<Sensor, string>(Sensors, x => x.Name ?? "").ToList(),
                _labelID    => Enumerable.OrderBy<Sensor, string>(Sensors, x => x.Id ?? "").ToList(),
                _labelState => Enumerable.OrderBy<Sensor, bool>(Sensors, x => x.State).ToList(),
                _           => Enumerable.ToList<Sensor>(Sensors)
            };
        } else {
            sorted = columnName switch {
                _labelName  => Enumerable.OrderByDescending<Sensor, string>(Sensors, x => x.Name ?? "").ToList(),
                _labelID    => Enumerable.OrderByDescending<Sensor, string>(Sensors, x => x.Id ?? "").ToList(),
                _labelState => Enumerable.OrderByDescending<Sensor, bool>(Sensors, x => x.State).ToList(),
                _           => Enumerable.ToList<Sensor>(Sensors)
            };
        }

        Sensors = new ObservableCollection<Sensor>(sorted);

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(Sensors));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals(_labelID) ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals(_labelName) ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals(_labelState) ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task ToggleSensorState(Sensor? sensor) {
        if (sensor == null) return;
        sensor.State = !sensor.State;
        if (!string.IsNullOrEmpty(sensor.Id) && IsConnected) {
            if (ConnectionService.Client is { } client) await client.SendSensorCmdAsync(sensor, sensor.State)!;
        }
    }

    [RelayCommand]
    private async Task RefreshSensorsAsync() {
        IsBusy = true;
        try {
            if (_profileService?.ActiveProfile is { } profile) profile.RefreshSensors();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}