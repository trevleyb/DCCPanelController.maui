using System.Collections.ObjectModel;
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
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class SensorsViewModel : AccessoryViewModel<Sensor>
{
    private const string _labelID    = "ID";
    private const string _labelName  = "Sensor";
    private const string _labelState = "Occupied?";

    private readonly ILogger<SensorsViewModel> _logger;

    [ObservableProperty] private bool _isSensorSelected;
    [ObservableProperty] private bool _canAddSensor;

    [ObservableProperty] private string _columnLabelID    = _labelID;
    [ObservableProperty] private string _columnLabelName  = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;

    private SfBottomSheet? _bottomSheet;

    public SensorsViewModel(ILogger<SensorsViewModel> logger, ProfileService profileService, ConnectionService connectionService)
        : base(profileService, connectionService)
    {
        _logger = logger;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SelectedSensor))
                IsSensorSelected = SelectedSensor != null;
        };
    }

    public ObservableCollection<Sensor> Sensors
    {
        get => Items;
        private set => Items = value;
    }

    [ObservableProperty] private Sensor? _selectedSensor;

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems()
    {
        // Sensors support is currently disabled in original code
        IsSupported = false;
        CanAddSensor = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }

    protected override string DefaultSortKey => _labelName;

    protected override ObservableCollection<Sensor> ResolveCollection(Profile profile) => profile.Sensors;

    protected override IReadOnlyDictionary<string, Func<Sensor, IComparable>> Sorters => new Dictionary<string, Func<Sensor, IComparable>>
    {
        [_labelName]  = x => x.Name ?? "",
        [_labelID]    = x => x.SystemId ?? "",
        [_labelState] = x => x.State
    };

    protected override void UpdateColumnLabels()
    {
        ColumnLabelID = LabelWithArrow(_labelID, _labelID);
        ColumnLabelName = LabelWithArrow(_labelName, _labelName);
        ColumnLabelState = LabelWithArrow(_labelState, _labelState);
    }

    protected override void OnItemsRebound() => OnPropertyChanged(nameof(Sensors));

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task ToggleSensorState(Sensor? sensor)
    {
        if (sensor == null) return;
        sensor.State = !sensor.State;
        if (!string.IsNullOrEmpty(sensor.SystemId))
        {
            if (ConnectionService.Client is { State: DccClientState.Connected } client)
                await client.SendSensorCmdAsync(sensor, sensor.State)!;
        }
    }

    [RelayCommand]
    private async Task RefreshSensorsAsync()
    {
        IsBusy = true;
        try
        {
            _profileService.ActiveProfile?.RefreshSensors();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteSensorAsync(Sensor? sensor)
    {
        sensor ??= SelectedSensor;
        if (sensor is null) return;
        Sensors.Remove(sensor);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Sensors));
        SelectedSensor = null;
        IsSensorSelected = false;
    }

    [RelayCommand]
    private async Task AddSensorAsync()
    {
        var sensor = new Sensor
        {
            SystemId = TableAnalyser<Sensor>.GetUniqueID(Sensors.ToList()),
            Name = "New Sensor",
            Source = AccessorySource.Manual,
        };
        Sensors.Add(sensor);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Sensors));
        SelectedSensor = null;
        IsSensorSelected = false;
    }

    [RelayCommand]
    private async Task EditSensorAsync(Sensor? sensor)
    {
        sensor ??= SelectedSensor;
        try
        {
            if (sensor is { } && _bottomSheet is { } sheet)
            {
                var vm = new SensorsEditViewModel(LogHelper.CreateLogger<SensorsEditViewModel>(), sensor, ConnectionService);
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
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error Launching Sensor Properties Page: " + ex.Message);
        }
        OnPropertyChanged(nameof(Sensors));
    }
}
