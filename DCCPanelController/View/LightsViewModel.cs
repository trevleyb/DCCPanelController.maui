using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;
using Syncfusion.Maui.Toolkit.BottomSheet;
using Light = DCCPanelController.Models.DataModel.Accessories.Light;

namespace DCCPanelController.View;

public partial class LightsViewModel : AccessoryViewModel<Light> {
    private const string _labelID    = "ID";
    private const string _labelName  = "Light";
    private const string _labelState = "Lit?";

    private readonly ILogger<LightsViewModel> _logger;

    [ObservableProperty] private Light? _selectedLight;
    [ObservableProperty] private bool   _isLightSelected;

    [ObservableProperty] private string _columnLabelID    = _labelID;
    [ObservableProperty] private string _columnLabelName  = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;

    private SfBottomSheet? _bottomSheet;

    public LightsViewModel(ILogger<LightsViewModel> logger, ProfileService profileService, ConnectionService connectionService)
        : base(profileService, connectionService) {
        _logger = logger;

        PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(SelectedLight))
                IsLightSelected = SelectedLight != null;
        };
    }

    public ObservableCollection<Light> Lights {
        get => Items;
        private set => Items = value;
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems() {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Lights) ?? false;
    }

    protected override string DefaultSortKey => _labelName;

    protected override ObservableCollection<Light> ResolveCollection(Profile profile) => profile.Lights;

    protected override IReadOnlyDictionary<string, Func<Light, IComparable>> Sorters =>
        new Dictionary<string, Func<Light, IComparable>> {
            [_labelName] = x => x.Name ?? "",
            [_labelID] = x => x.SystemId ?? "",
            [_labelState] = x => x.State
        };

    protected override void UpdateColumnLabels() {
        ColumnLabelID = LabelWithArrow(_labelID, _labelID);
        ColumnLabelName = LabelWithArrow(_labelName, _labelName);
        ColumnLabelState = LabelWithArrow(_labelState, _labelState);
    }

    protected override void OnItemsRebound() => OnPropertyChanged(nameof(Lights));

    [RelayCommand]
    public async Task ToggleLightState(Light? light) {
        if (light == null) return;
        light.State = !light.State;
        if (!string.IsNullOrEmpty(light.SystemId)) {
            if (ConnectionService.Client is { State: DccClientState.Connected } client)
                await client.SendLightCmdAsync(light, light.State)!;
        }
    }

    [RelayCommand]
    private async Task RefreshLightsAsync() {
        IsBusy = true;
        try {
            _profileService.ActiveProfile?.RefreshLights();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
            OnPropertyChanged(nameof(Lights));
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteLightAsync(Light? light) {
        light ??= SelectedLight;
        if (light is null) return;
        Lights.Remove(light);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Lights));
        SelectedLight = null;
        IsLightSelected = false;
    }

    [RelayCommand]
    private async Task AddLightAsync() {
        var light = new Light {
            SystemId = TableAnalyser<Light>.GetUniqueID(Lights.ToList()),
            Name = "New Light",
            Source = AccessorySource.Manual,
        };

        Lights.Add(light);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Lights));
        SelectedLight = null;
        IsLightSelected = false;
    }

    [RelayCommand]
    public async Task EditLightAsync(Light? light) {
        light ??= SelectedLight;
        try {
            if (light is { } && _bottomSheet is { } sheet) {
                var vm = new LightsEditViewModel(LogHelper.CreateLogger<LightsEditViewModel>(), light, ConnectionService);
                sheet.BottomSheetContent = vm.CreatePropertiesView();
                sheet.ContentWidthMode = (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
                    ? BottomSheetContentWidthMode.Full
                    : BottomSheetContentWidthMode.Custom;

                if (sheet.ContentWidthMode == BottomSheetContentWidthMode.Custom)
                    sheet.BottomSheetContentWidth = 400;

                sheet.ShowGrabber = true;
                sheet.EnableSwiping = true;
                sheet.CollapsedHeight = 0;
                sheet.CollapseOnOverlayTap = true;
                sheet.State = BottomSheetState.HalfExpanded;
                sheet.IsModal = true;
                sheet.IsVisible = true;
                sheet.Show();
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Light Properties Page: " + ex.Message);
        }

        OnPropertyChanged(nameof(Lights));
    }
}