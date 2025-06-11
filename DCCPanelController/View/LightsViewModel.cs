using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class LightsViewModel : Base.ConnectionViewModel {
    private const string _labelID = "ID";
    private const string _labelName = "Light";
    private const string _labelState = "Lit?";

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    [ObservableProperty] private ObservableCollection<Light> _lights;
    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;
    private bool _isAscending;
    private string _sortColumn = "";

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    public LightsViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        Lights = Profile.Lights;
        IsSupported = profile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapabilitiesEnum.Lights) ?? false;
        SetLabels();
    }

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
            await ConnectionService.SendLightCmdAsync(light, light.State )!;
        }
    }
    
    [RelayCommand]
    private async Task RefreshLightsAsync() {
        IsBusy = true;
        try {
            for (var ptr = Profile.Lights.Count; ptr > 0; ptr--) {
                Profile.Lights.RemoveAt(ptr - 1);
            }
            await ConnectionService.ForceRefresh();
            OnPropertyChanged(nameof(Lights));
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}