using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.Views.Base;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.Views;

public partial class TurnoutsViewModel : ConnectionViewModel {
    private const string _labelID = "ID";
    private const string _labelName = "User Name";
    private const string _labelState = "State";
    private const string _labelAddress = "DCC Address";

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;
    public string LabelAddress => _labelAddress;

    [ObservableProperty] private Turnout? _selectedTurnout;
    [ObservableProperty] private bool _isTurnoutSelected;
    [ObservableProperty] private bool _canAddTurnout;

    [ObservableProperty] private string _columnLabelAddress = _labelAddress;
    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts = [];

    private bool _isAscending;
    private string _sortColumn = "";

    public bool IsSupported { get; set; }
    public bool IsNotSupported => !IsSupported;

    public double ScreenHeight = 100;
    public double ScreenWidth = 100;

    private ProfileService _profileService;
    private ILogger<TurnoutsViewModel> _logger;
    private SfBottomSheet? _bottomSheet;
    
    public TurnoutsViewModel(ILogger<TurnoutsViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;        
        _profileService = profileService;
        
        _profileService.ActiveProfileChanged += (sender, args) => {
            // If the profile has changed, we need to reset the Turnout Data Collection
            Turnouts = _profileService?.ActiveProfile?.Turnouts ?? throw new ArgumentNullException(nameof(profileService),"TurnoutViewModel: Active profile is not defined.");
            SetLabels();
        };
        
        Turnouts = _profileService?.ActiveProfile?.Turnouts ?? throw new ArgumentNullException(nameof(profileService),"TurnoutViewModel: Active profile is not defined.");
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedTurnout)) {
                IsTurnoutSelected = SelectedTurnout != null;
            }
        };
        SetLabels();
    }

    public void SetNavigationReferences(SfBottomSheet bottomSheet) {
        _bottomSheet = bottomSheet;
    }

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
                _labelName    => Enumerable.OrderBy<Turnout, string>(Turnouts, x => x.Name ?? "").ToList(),
                _labelAddress => Enumerable.OrderBy<Turnout, string>(Turnouts, x => x.DccAddress.ToString()).ToList(),
                _labelID      => Enumerable.OrderBy<Turnout, string>(Turnouts, x => x.Id ?? "").ToList(),
                _labelState   => Enumerable.OrderBy<Turnout, TurnoutStateEnum>(Turnouts, x => x.State).ToList(),
                _             => Enumerable.ToList<Turnout>(Turnouts)
            };
        } else {
            sortedTurnout = columnName switch {
                _labelName    => Enumerable.OrderByDescending<Turnout, string>(Turnouts, x => x.Name ?? "").ToList(),
                _labelID      => Enumerable.OrderByDescending<Turnout, string>(Turnouts, x => x.Id ?? "").ToList(),
                _labelAddress => Enumerable.OrderByDescending<Turnout, string>(Turnouts, x => x.DccAddress.ToString()).ToList(),
                _labelState   => Enumerable.OrderByDescending<Turnout, TurnoutStateEnum>(Turnouts, x => x.State).ToList(),
                _             => Enumerable.ToList<Turnout>(Turnouts)
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
        if (turnout is not null) {
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
            Id = TurnoutAnalyzer.GetUniqueID(Enumerable.ToList<Turnout>(Turnouts)),
            Name = "New Turnout",
            State = TurnoutStateEnum.Closed,
            Default = TurnoutStateEnum.Closed,
            IsEditable = true
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
        if (turnout is not null) {
            if (IsConnected) {
                if (ConnectionService.Client is { } client) await client.SendTurnoutCmdAsync(turnout, turnout.State == TurnoutStateEnum.Thrown)!;
            }
            OnPropertyChanged(nameof(Turnouts));
        }
    }

    [RelayCommand]
    public async Task EditTurnoutAsync(Turnout? turnout) {
        turnout ??= SelectedTurnout;
        try {
            if (turnout is not null && _bottomSheet is { } sfBottomSheet) {
                var turnoutsEditViewModel = new TurnoutsEditViewModel(LogHelper.CreateLogger<TurnoutsEditViewModel>(), turnout, ConnectionService);

                if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Current.Idiom == DeviceIdiom.Phone) {
                    _bottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Full;
                } else {
                    _bottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Custom;
                    _bottomSheet.BottomSheetContentWidth = 400;
                }

                _bottomSheet.BottomSheetContent = new TurnoutsEditView(LogHelper.CreateLogger<TurnoutsEditView>(), turnoutsEditViewModel);
                _bottomSheet.ShowGrabber = true;
                _bottomSheet.EnableSwiping = true;
                _bottomSheet.CollapsedHeight = 0;
                _bottomSheet.CollapseOnOverlayTap = true;
                _bottomSheet.State = BottomSheetState.HalfExpanded;
                _bottomSheet.IsModal = true;
                _bottomSheet.Show();
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        }
        OnPropertyChanged(nameof(Turnouts));
    }
}