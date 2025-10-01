using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class TurnoutsViewModel : ConnectionViewModel {
    private const    string                     _labelID      = "ID";
    private const    string                     _labelName    = "User Name";
    private const    string                     _labelState   = "State";
    private const    string                     _labelAddress = "DCC Address";
    private readonly ILogger<TurnoutsViewModel> _logger;

    private readonly             ProfileService _profileService;
    [ObservableProperty] private bool           _canAddTurnout;

    [ObservableProperty] private string _columnLabelAddress = _labelAddress;
    [ObservableProperty] private string _columnLabelID      = _labelID;
    [ObservableProperty] private string _columnLabelName    = _labelName;
    [ObservableProperty] private string _columnLabelState   = _labelState;

    private                      bool _isAscending;
    [ObservableProperty] private bool _isTurnoutSelected;

    [ObservableProperty] private Turnout?                      _selectedTurnout;
    private                      string                        _sortColumn = "";
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts   = [];

    public double ScreenHeight = 100;
    public double ScreenWidth  = 100;

    public TurnoutsViewModel(ILogger<TurnoutsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;

        _profileService.ActiveProfileChanged += (sender, args) => {
            // If the profile has changed, we need to reset the Turnout Data Collection
            Turnouts = _profileService?.ActiveProfile?.Turnouts ?? throw new ArgumentNullException(nameof(profileService), "TurnoutViewModel: Active profile is not defined.");
            SetLabels();
        };

        Turnouts = _profileService?.ActiveProfile?.Turnouts ?? throw new ArgumentNullException(nameof(profileService), "TurnoutViewModel: Active profile is not defined.");
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedTurnout)) {
                IsTurnoutSelected = SelectedTurnout != null;
            }
        };
        SetLabels();
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;
    public string LabelAddress => _labelAddress;

    public bool IsSupported { get; set; }
    public bool IsNotSupported => !IsSupported;

    private SfBottomSheet? _bottomSheet;
    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems() {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Turnouts) ?? false;
        CanAddTurnout = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals(LabelID) ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals(LabelName) ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals(LabelState) ? _isAscending.GetSortDirection() : "");
        ColumnLabelAddress = LabelAddress + (_sortColumn.Equals(LabelAddress) ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    private async Task RefreshTurnoutsAsync() {
        IsBusy = true;
        try {
            if (_profileService?.ActiveProfile is { } profile) profile.RefreshTurnouts();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
            OnPropertyChanged(nameof(Turnouts));
            SelectedTurnout = null;
            IsTurnoutSelected = false;
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Turnout> sortedTurnout;

        if (!_isAscending) {
            sortedTurnout = columnName switch {
                _labelName    => Turnouts.OrderBy<Turnout, string>(x => x.Name ?? "").ToList(),
                _labelAddress => Turnouts.OrderBy<Turnout, string>(x => x.DccAddress.ToString()).ToList(),
                _labelID      => Turnouts.OrderBy<Turnout, string>(x => x.Id ?? "").ToList(),
                _labelState   => Turnouts.OrderBy<Turnout, TurnoutStateEnum>(x => x.State).ToList(),
                _             => Turnouts.ToList<Turnout>(),
            };
        } else {
            sortedTurnout = columnName switch {
                _labelName    => Turnouts.OrderByDescending<Turnout, string>(x => x.Name ?? "").ToList(),
                _labelID      => Turnouts.OrderByDescending<Turnout, string>(x => x.Id ?? "").ToList(),
                _labelAddress => Turnouts.OrderByDescending<Turnout, string>(x => x.DccAddress.ToString()).ToList(),
                _labelState   => Turnouts.OrderByDescending<Turnout, TurnoutStateEnum>(x => x.State).ToList(),
                _             => Turnouts.ToList<Turnout>(),
            };
        }
        _sortColumn = columnName;
        _isAscending = !_isAscending;

        Turnouts = new ObservableCollection<Turnout>(sortedTurnout);
        OnPropertyChanged(nameof(Turnouts));
        SetLabels();
    }

    [RelayCommand]
    private async Task DeleteTurnoutAsync(Turnout? turnout) {
        turnout ??= SelectedTurnout;
        if (turnout is { }) {
            Turnouts.Remove(turnout);
            OnPropertyChanged(nameof(Turnouts));
            await _profileService.SaveAsync();
            SelectedTurnout = null;
            IsTurnoutSelected = false;
        }
    }

    [RelayCommand]
    private async Task AddTurnoutAsync() {
        var turnout = new Turnout {
            Id = TableAnalyser<Turnout>.GetUniqueID(Turnouts.ToList<Turnout>()),
            Name = "New Turnout",
            State = TurnoutStateEnum.Closed,
            Default = TurnoutStateEnum.Closed,
            IsEditable = true,
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
        if (turnout is { }) {
            if (ConnectionService.Client is { State: DccClientState.Connected } client) await client.SendTurnoutCmdAsync(turnout, turnout.State == TurnoutStateEnum.Thrown)!;
            OnPropertyChanged(nameof(Turnouts));
        }
    }

    [RelayCommand]
    public async Task EditTurnoutAsync(Turnout? turnout) {
        turnout ??= SelectedTurnout;
        try {
            if (turnout is { } && _bottomSheet is { } sfBottomSheet) {
                var turnoutsEditViewModel = new TurnoutsEditViewModel(LogHelper.CreateLogger<TurnoutsEditViewModel>(), turnout, ConnectionService);
                sfBottomSheet.BottomSheetContent = turnoutsEditViewModel.CreatePropertiesView(); //new TurnoutsEditView(LogHelper.CreateLogger<TurnoutsEditView>(), turnoutsEditViewModel);

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
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        }
        OnPropertyChanged(nameof(Turnouts));
    }
}