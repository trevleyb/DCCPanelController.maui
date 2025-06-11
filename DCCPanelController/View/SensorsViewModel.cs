using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class SensorsViewModel : Base.ConnectionViewModel {
    private const string _labelID = "ID";
    private const string _labelName = "Sensor";
    private const string _labelState = "Occupied?";

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    [ObservableProperty] private ObservableCollection<Sensor> _sensors;
    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;

    private bool _isAscending;
    private string _sortColumn = "";

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    public SensorsViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        Sensors = Profile.Sensors;
        IsSupported = profile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapabilities.Sensors) ?? false;
        SetLabels();
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Sensor> sorted;
        if (!_isAscending) {
            sorted = columnName switch {
                _labelName  => Sensors.OrderBy<Sensor, string>(x => x.Name ?? "").ToList(),
                _labelID    => Sensors.OrderBy<Sensor, string>(x => x.Id ?? "").ToList(),
                _labelState => Sensors.OrderBy<Sensor, bool>(x => x.State).ToList(),
                _           => Sensors.ToList<Sensor>()
            };
        } else {
            sorted = columnName switch {
                _labelName  => Sensors.OrderByDescending<Sensor, string>(x => x.Name ?? "").ToList(),
                _labelID    => Sensors.OrderByDescending<Sensor, string>(x => x.Id ?? "").ToList(),
                _labelState => Sensors.OrderByDescending<Sensor, bool>(x => x.State).ToList(),
                _           => Sensors.ToList<Sensor>()
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
            await ConnectionService.SendSensorCmdAsync(sensor, sensor.State )!;
        }
    }
    
    [RelayCommand]
    private async Task RefreshSensorsAsync() {
        IsBusy = true;
        try {
            for (var ptr = Profile.Sensors.Count; ptr > 0; ptr--) {
                Profile.Blocks.RemoveAt(ptr - 1);
            }
            await ConnectionService.ForceRefresh();
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}