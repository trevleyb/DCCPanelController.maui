using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class TurnoutsViewModel : BaseViewModel {
    private const string LabelID = "ID";
    private const string LabelName = "Turnout";
    private const string LabelState = "State";

    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;

    private bool _isAscending;
    private string _sortColumn = "";
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;

    public TurnoutsViewModel(TurnoutsService turnoutService, ConnectionService connectionService, NavigationService navigationService) {
        TurnoutService = turnoutService;
        ConnectionService = connectionService;
        NavigationService = navigationService;
        Turnouts = TurnoutService.Turnouts ?? [];
        SetLabels();
    }

    private NavigationService NavigationService { get; }
    private ConnectionService ConnectionService { get; }
    private TurnoutsService TurnoutService { get; }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Turnout> sortedTurnout;

        if (!_isAscending) {
            sortedTurnout = columnName.ToLower() switch {
                "name"  => Turnouts.OrderBy<Turnout, string>(x => x.Name ?? "").ToList(),
                "id"    => Turnouts.OrderBy<Turnout, string>(x => x.Id ?? "").ToList(),
                "state" => Turnouts.OrderBy<Turnout, TurnoutStateEnum>(x => x.State).ToList(),
                _       => Turnouts.ToList<Turnout>()
            };
        } else {
            sortedTurnout = columnName.ToLower() switch {
                "name"  => Turnouts.OrderByDescending<Turnout, string>(x => x.Name ?? "").ToList(),
                "id"    => Turnouts.OrderByDescending<Turnout, string>(x => x.Id ?? "").ToList(),
                "state" => Turnouts.OrderByDescending<Turnout, TurnoutStateEnum>(x => x.State).ToList(),
                _       => Turnouts.ToList<Turnout>()
            };
        }

        _sortColumn = columnName;
        _isAscending = !_isAscending;

        Turnouts = new ObservableCollection<Turnout>(sortedTurnout);
        OnPropertyChanged(nameof(Turnouts));
        SetLabels();
    }

    [RelayCommand]
    private async Task EditTurnoutAsync(Turnout? turnout) {
        await NavigationService.NavigateToEditTurnoutAsync(turnout);
        OnPropertyChanged(nameof(Turnouts));
    }

    [RelayCommand]
    private async Task DeleteTurnoutAsync(Turnout turnout) {
        Turnouts.Remove(turnout);
        OnPropertyChanged(nameof(Turnouts));
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

        var result = await NavigationService.NavigateToEditTurnoutAsync(turnout);
        if (result is not null) Turnouts.Add(result);
        OnPropertyChanged(nameof(Turnouts));
    }

    [RelayCommand]
    private async Task SendTurnoutStateAsync(Turnout? turnout) {
        if (turnout == null) return;
        if (!string.IsNullOrEmpty(turnout.Id)) ConnectionService?.SendTurnoutStateChangeCommand(turnout.Id, turnout.State);
        OnPropertyChanged(nameof(Turnouts));
    }

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync(Turnout? turnout) {
        if (turnout == null) return;

        turnout.State = turnout.State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };

        if (!string.IsNullOrEmpty(turnout.Id)) ConnectionService?.SendTurnoutStateChangeCommand(turnout.Id, turnout.State);
        OnPropertyChanged(nameof(Turnouts));
    }
}