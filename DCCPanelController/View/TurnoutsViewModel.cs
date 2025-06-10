using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using DCCPanelController.View.Properties;

namespace DCCPanelController.View;

public partial class TurnoutsViewModel : ConnectionViewModel {
    private const string _labelID = "System Name";
    private const string _labelName = "User Name";
    private const string _labelState = "State";
    private const string _labelAddress = "DCC Address";

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;
    public string LabelAddress => _labelAddress;

    [ObservableProperty] private bool _isTurnoutSelected;
    [ObservableProperty] private Turnout? _selectedTurnout;
    [ObservableProperty] private bool _canAddTurnout;

    [ObservableProperty] private string _columnLabelAddress = _labelAddress;
    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;

    public INavigation? Navigation;
    private bool _isAscending;
    private string _sortColumn = "";

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    public double ScreenHeight = 100;
    public double ScreenWidth = 100;

    public TurnoutsViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        Turnouts = Profile.Turnouts;
        IsSupported = profile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapabilities.Turnouts) ?? false;
        CanAddTurnout = profile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedTurnout)) {
                IsTurnoutSelected = SelectedTurnout != null;
            }
        };
        SetLabels();
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
            var removeTurnouts = Profile.Turnouts.Where(turnout => turnout.IsEditable == false).ToList();
            foreach (var turnout in removeTurnouts) {
                Profile.Turnouts.Remove(turnout);
            }
            await RefreshTurnoutsAsync();
            OnPropertyChanged(nameof(Turnouts));
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
                _             => Turnouts.ToList<Turnout>()
            };
        } else {
            sortedTurnout = columnName switch {
                _labelName    => Turnouts.OrderByDescending<Turnout, string>(x => x.Name ?? "").ToList(),
                _labelID      => Turnouts.OrderByDescending<Turnout, string>(x => x.Id ?? "").ToList(),
                _labelAddress => Turnouts.OrderByDescending<Turnout, string>(x => x.DccAddress.ToString()).ToList(),
                _labelState   => Turnouts.OrderByDescending<Turnout, TurnoutStateEnum>(x => x.State).ToList(),
                _             => Turnouts.ToList<Turnout>()
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
            await Profile.SaveAsync();
        }
    }

    [RelayCommand]
    private async Task AddTurnoutAsync() {
        var turnout = new Turnout {
            Id = TurnoutAnalyzer.GetUniqueID(Turnouts.ToList<Turnout>()),
            Name = "New Turnout",
            State = TurnoutStateEnum.Closed,
            Default = TurnoutStateEnum.Closed,
            IsEditable = true
        };
        Turnouts.Add(turnout);
        await EditTurnoutAsync(turnout);
        await Profile.SaveAsync();
        OnPropertyChanged(nameof(Turnouts));
    }

    [RelayCommand]
    private async Task SendTurnoutStateAsync(Turnout? turnout) {
        if (turnout is not null) {
            if (IsConnected) {
                await ConnectionService?.SendTurnoutCmdAsync(turnout, turnout.State == TurnoutStateEnum.Thrown)!;
            }
            OnPropertyChanged(nameof(Turnouts));
        }
    }

    [RelayCommand]
    public async Task EditTurnoutAsync(Turnout? turnout) {
        turnout ??= SelectedTurnout;
        try {
            if (turnout is not null && Navigation is { } navigation) {
                var turnoutsEditViewModel = new TurnoutsEditViewModel(turnout, ConnectionService);
                await PropertyDisplayService.ShowPropertiesAsync(navigation, turnoutsEditViewModel, ScreenWidth, ScreenHeight);
                await Profile.SaveAsync();
                OnPropertyChanged(nameof(Turnouts));
            }
        } catch (Exception ex) {
            Console.WriteLine("Error Launching Panel Properties Page: " + ex.Message);
        }
    }
}