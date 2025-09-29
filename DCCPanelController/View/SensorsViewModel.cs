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
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class SensorsViewModel : ConnectionViewModel {
    private const                string         _labelID    = "ID";
    private const                string         _labelName  = "Sensor";
    private const                string         _labelState = "Occupied?";
    private readonly             ProfileService _profileService;
    [ObservableProperty] private string         _columnLabelID    = _labelID;
    [ObservableProperty] private string         _columnLabelName  = _labelName;
    [ObservableProperty] private string         _columnLabelState = _labelState;

    [ObservableProperty] private bool _isSensorSelected;
    [ObservableProperty] private bool _canAddSensor;
    private                      bool _isAscending;

    private ILogger<SensorsViewModel> _logger;

    [ObservableProperty] private Sensor?                      _selectedSensor;
    [ObservableProperty] private ObservableCollection<Sensor> _sensors;
    private                      string                       _sortColumn = "";

    public SensorsViewModel(ILogger<SensorsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (sender, args) => {
            Sensors = _profileService?.ActiveProfile?.Sensors ?? throw new ArgumentNullException(nameof(profileService), "SensorsViewModel: Active profile is not defined.");
            IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Sensors) ?? false;
            SetLabels();
        };

        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedSensor)) {
                IsSensorSelected = SelectedSensor != null;
            }
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

    private SfBottomSheet? _bottomSheet;
    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems() {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Sensors) ?? false;
        CanAddSensor = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }
    
    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Sensor> sorted;
        if (!_isAscending) {
            sorted = columnName switch {
                _labelName  => Sensors.OrderBy<Sensor, string>(x => x.Name ?? "").ToList(),
                _labelID    => Sensors.OrderBy<Sensor, string>(x => x.Id ?? "").ToList(),
                _labelState => Sensors.OrderBy<Sensor, bool>(x => x.State).ToList(),
                _           => Sensors.ToList<Sensor>(),
            };
        } else {
            sorted = columnName switch {
                _labelName  => Sensors.OrderByDescending<Sensor, string>(x => x.Name ?? "").ToList(),
                _labelID    => Sensors.OrderByDescending<Sensor, string>(x => x.Id ?? "").ToList(),
                _labelState => Sensors.OrderByDescending<Sensor, bool>(x => x.State).ToList(),
                _           => Sensors.ToList<Sensor>(),
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

    [RelayCommand]
    private async Task DeleteSensorAsync(Sensor? sensor) {
        sensor ??= SelectedSensor;
        if (sensor is { }) {
            Sensors.Remove(sensor);
            OnPropertyChanged(nameof(Sensors));
            await _profileService.SaveAsync();
            SelectedSensor = null;
            IsSensorSelected = false;
        }
    }

    [RelayCommand]
    private async Task AddSensorAsync() {
        var sensor = new Sensor {
            Id = TableAnalyser<Sensor>.GetUniqueID(Sensors.ToList<Sensor>()),
            Name = "New Sensor",
            IsEditable = true,
        };
        Sensors.Add(sensor);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Sensors));
        SelectedSensor = null;
        IsSensorSelected = false;
    }

    [RelayCommand]
    private async Task SendSensorStateAsync(Sensor? sensor) {
        if (sensor is { }) {
            if (IsConnected) {
                if (ConnectionService.Client is { } client) await client.SendSensorCmdAsync(sensor, sensor.State)!;
            }
            OnPropertyChanged(nameof(Sensors));
        }
    }

    [RelayCommand]
    public async Task EditSensorAsync(Sensor? sensor) {
        sensor ??= SelectedSensor;
        try {
            if (sensor is { } && _bottomSheet is { } sfBottomSheet) {
                var sensorsEditViewModel = new SensorsEditViewModel(LogHelper.CreateLogger<SensorsEditViewModel>(), sensor, ConnectionService);
                sfBottomSheet.BottomSheetContent = sensorsEditViewModel.CreatePropertiesView();
        
                if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Current.Idiom == DeviceIdiom.Phone) {
                    sfBottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Full;
                } else {
                    sfBottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Custom;
                    sfBottomSheet.BottomSheetContentWidth = 400;
                }
        
                sfBottomSheet.ShowGrabber = true;
                sfBottomSheet.EnableSwiping = true;
                sfBottomSheet.CollapsedHeight = 0;
                sfBottomSheet.CollapseOnOverlayTap = true;
                sfBottomSheet.State = BottomSheetState.HalfExpanded;
                sfBottomSheet.IsModal = true;
                sfBottomSheet.IsVisible = true;
                sfBottomSheet.Show();
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Sensor Properties Page: " + ex.Message);
        }
        OnPropertyChanged(nameof(Sensors));
    }
}