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

public partial class LightsViewModel : ConnectionViewModel {
    private const                string         _labelID    = "ID";
    private const                string         _labelName  = "Light";
    private const                string         _labelState = "Lit?";
    
    private readonly             ProfileService _profileService;
    [ObservableProperty] private string         _columnLabelID    = _labelID;
    [ObservableProperty] private string         _columnLabelName  = _labelName;
    [ObservableProperty] private string         _columnLabelState = _labelState;
    private                      bool           _isAscending;

    [ObservableProperty] private Light?                      _selectedLight;
    [ObservableProperty] private ObservableCollection<Light> _lights;
    private                      ILogger<LightsViewModel>    _logger;
    private                      string                      _sortColumn = "";

    [ObservableProperty] private bool _isLightSelected;
    [ObservableProperty] private bool _canAddLight;

    public LightsViewModel(ILogger<LightsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (sender, args) => {
            Lights = _profileService?.ActiveProfile?.Lights ?? throw new ArgumentNullException(nameof(profileService), "LightsViewModel: Active profile is not defined.");
            IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Lights) ?? false;
            SetLabels();
        };
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedLight)) {
                IsLightSelected = SelectedLight != null;
            }
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

    private SfBottomSheet? _bottomSheet;
    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems() {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Turnouts) ?? false;
        CanAddLight = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }
    
    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Light> sorted;
        if (!_isAscending) {
            sorted = columnName switch {
                _labelName  => Lights.OrderBy<Light, string>(x => x.Name ?? "").ToList(),
                _labelID    => Lights.OrderBy<Light, string>(x => x.Id ?? "").ToList(),
                _labelState => Lights.OrderBy<Light, bool>(x => x.State).ToList(),
                _           => Lights.ToList<Light>(),
            };
        } else {
            sorted = columnName switch {
                _labelName  => Lights.OrderByDescending<Light, string>(x => x.Name ?? "").ToList(),
                _labelID    => Lights.OrderByDescending<Light, string>(x => x.Id ?? "").ToList(),
                _labelState => Lights.OrderByDescending<Light, bool>(x => x.State).ToList(),
                _           => Lights.ToList<Light>(),
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
    
        [RelayCommand]
    private async Task DeleteLightAsync(Light? light) {
        light ??= SelectedLight;
        if (light is { }) {
            Lights.Remove(light);
            OnPropertyChanged(nameof(Lights));
            await _profileService.SaveAsync();
            SelectedLight = null;
            IsLightSelected = false;
        }
    }

    [RelayCommand]
    private async Task AddLightAsync() {
        var light = new Light {
            Id = TableAnalyser<Light>.GetUniqueID(Lights.ToList<Light>()),
            Name = "New Light",
            IsEditable = true,
        };
        Lights.Add(light);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Lights));
        SelectedLight = null;
        IsLightSelected = false;
    }

    [RelayCommand]
    private async Task SendLightStateAsync(Light? light) {
        if (light is { }) {
            if (IsConnected) {
                if (ConnectionService.Client is { } client) await client.SendLightCmdAsync(light, light.State)!;
            }
            OnPropertyChanged(nameof(Lights));
        }
    }

    [RelayCommand]
    public async Task EditLightAsync(Light? light) {
        light ??= SelectedLight;
        try {
            if (light is { } && _bottomSheet is { } sfBottomSheet) {
                var lightssEditViewModel = new LightsEditViewModel(LogHelper.CreateLogger<LightsEditViewModel>(), light, ConnectionService);
        
                if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Current.Idiom == DeviceIdiom.Phone) {
                    _bottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Full;
                } else {
                    _bottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Custom;
                    _bottomSheet.BottomSheetContentWidth = 400;
                }
        
                _bottomSheet.BottomSheetContent = new LightsEditView(LogHelper.CreateLogger<LightsEditView>(), lightssEditViewModel);
                _bottomSheet.ShowGrabber = true;
                _bottomSheet.EnableSwiping = true;
                _bottomSheet.CollapsedHeight = 0;
                _bottomSheet.CollapseOnOverlayTap = true;
                _bottomSheet.State = BottomSheetState.HalfExpanded;
                _bottomSheet.IsModal = true;
                _bottomSheet.IsVisible = true;
                _bottomSheet.Show();
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Light Properties Page: " + ex.Message);
        }
        OnPropertyChanged(nameof(Lights));
    }
}