using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class TurnoutsViewModel : AccessoryViewModel<Turnout> {
    private const string _labelID      = "ID";
    private const string _labelName    = "User Name";
    private const string _labelState   = "State";
    private const string _labelAddress = "DCC Address";

    private readonly ILogger<TurnoutsViewModel> _logger;

    [ObservableProperty] private bool _canDelTurnout;
    [ObservableProperty] private bool _canAddTurnout;
    [ObservableProperty] private bool _isTurnoutSelected;

    // Header labels with sort arrows
    [ObservableProperty] private string _columnLabelAddress = _labelAddress;
    [ObservableProperty] private string _columnLabelID      = _labelID;
    [ObservableProperty] private string _columnLabelName    = _labelName;
    [ObservableProperty] private string _columnLabelState   = _labelState;

    private SfBottomSheet? _bottomSheet;

    public TurnoutsViewModel(ILogger<TurnoutsViewModel> logger, ProfileService profileService, ConnectionService connectionService)
        : base(profileService, connectionService) {
        _logger = logger;

        PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(SelectedTurnout)) {
                IsTurnoutSelected = SelectedTurnout != null;
                CanDelTurnout = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
            }
        };
    }

    // Alias for Items so XAML bindings remain unchanged
    public ObservableCollection<Turnout> Turnouts {
        get => Items;
        private set => Items = value;
    }

    [ObservableProperty] private Turnout? _selectedTurnout;

    public double ScreenHeight = 100;
    public double ScreenWidth  = 100;

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;
    public string LabelAddress => _labelAddress;

    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems() {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Turnouts) ?? false;
        CanAddTurnout = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
        CanDelTurnout = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }

    protected override string DefaultSortKey => _labelName;

    protected override ObservableCollection<Turnout> ResolveCollection(Profile profile) => profile.Turnouts;

    protected override IReadOnlyDictionary<string, Func<Turnout, IComparable>> Sorters =>
        new Dictionary<string, Func<Turnout, IComparable>> {
            [_labelName] = x => x.Name ?? "",
            [_labelID] = x => x.SystemId ?? "",
            [_labelAddress] = x => x.DccAddress.ToString() ?? "",
            [_labelState] = x => x.State
        };

    protected override void UpdateColumnLabels() {
        ColumnLabelID = LabelWithArrow(_labelID, _labelID);
        ColumnLabelName = LabelWithArrow(_labelName, _labelName);
        ColumnLabelState = LabelWithArrow(_labelState, _labelState);
        ColumnLabelAddress = LabelWithArrow(_labelAddress, _labelAddress);
    }

    protected override void OnItemsRebound() => OnPropertyChanged(nameof(Turnouts));

    [RelayCommand]
    private async Task RefreshTurnoutsAsync() {
        IsBusy = true;
        try {
            _profileService.ActiveProfile?.RefreshTurnouts();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
            OnPropertyChanged(nameof(Turnouts));
            SelectedTurnout = null;
            IsTurnoutSelected = false;
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteTurnoutAsync(Turnout? turnout) {
        turnout ??= SelectedTurnout;
        if (turnout is null) return;
        Turnouts.Remove(turnout);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Turnouts));
        SelectedTurnout = null;
        IsTurnoutSelected = false;
    }

    [RelayCommand]
    private async Task AddTurnoutAsync() {
        var turnout = new Turnout {
            SystemId = TableAnalyser<Turnout>.GetUniqueID(Turnouts.ToList()),
            Name = "New Turnout",
            State = TurnoutStateEnum.Closed,
            Default = TurnoutStateEnum.Closed,
            Source = AccessorySource.Manual,
        };

        Turnouts.Add(turnout);
        await EditTurnoutAsync(turnout);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Turnouts));
        SelectedTurnout = null;
        IsTurnoutSelected = false;
    }

    [RelayCommand]
    private async Task SendTurnoutStateAsync(Turnout? turnout) {
        if (turnout is null) return;
        if (ConnectionService.Client is { State: DccClientState.Connected } client)
            await client.SendTurnoutCmdAsync(turnout, turnout.State == TurnoutStateEnum.Thrown)!;

        OnPropertyChanged(nameof(Turnouts));
    }

    [RelayCommand]
    public async Task EditTurnoutAsync(Turnout? turnout) {
        turnout ??= SelectedTurnout;
        try {
            if (turnout is { } && _bottomSheet is { } sheet) {
                var vm = new TurnoutsEditViewModel(LogHelper.CreateLogger<TurnoutsEditViewModel>(), turnout, ConnectionService);
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
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        }

        OnPropertyChanged(nameof(Turnouts));
    }
}