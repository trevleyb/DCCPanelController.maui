using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace DCCPanelController.ViewModel;

public partial class TurnoutsViewModel : BaseViewModel {

    private ConnectionService? ConnectionService { get; }
    private bool _isAscending = false;
    private string _sortColumn = "";

    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;
    [ObservableProperty] private bool _canToggleTurnoutState;
    [ObservableProperty] private string _columnLabelID = "ID";
    [ObservableProperty] private string _columnLabelName = "Turnout SystemName";
    [ObservableProperty] private string _columnLabelState = "State";

    public TurnoutsViewModel(TurnoutsService? turnoutStateService, ConnectionService? connectionService) {
        ConnectionService = connectionService;
        Turnouts = turnoutStateService?.Turnouts ?? [];
        CanToggleTurnoutState = ConnectionService is not null && ConnectionService.IsConnected;
        SetLabels();
    }

    [RelayCommand(CanExecute = nameof(CanToggleTurnoutState))]
    public async Task SortByColumn(string columnName) {
        List<Turnout> sortedTurnout;
        if (!_isAscending) {
            sortedTurnout = columnName.ToLower() switch {
                "name"  => Turnouts.OrderBy(x => x.Name).ToList(),
                "id"    => Turnouts.OrderBy(x => x.Id).ToList(),
                "state" => Turnouts.OrderBy(x => x.State).ToList(),
                _       => Turnouts.ToList()
            };
        } else {
            sortedTurnout = columnName.ToLower() switch {
                "name"  => Turnouts.OrderByDescending(x => x.Name).ToList(),
                "id"    => Turnouts.OrderByDescending(x => x.Id).ToList(),
                "state" => Turnouts.OrderByDescending(x => x.State).ToList(),
                _       => Turnouts.ToList()
            };
        }

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        Turnouts = new ObservableCollection<Turnout>(sortedTurnout);
        OnPropertyChanged(nameof(Turnouts));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = "ID" + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = "Turnout SystemName" + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = "State" + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    public async Task ToggleTurnoutState(Turnout? turnout) {
        if (turnout == null) return;
        turnout.State = turnout.State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };
        if (!string.IsNullOrEmpty(turnout.Id)) ConnectionService?.SendTurnoutStateChangeCommand(turnout.Id, turnout.State);
    }
}