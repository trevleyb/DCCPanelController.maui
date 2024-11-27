using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.Services.NavigationService;
using DCCPanelController.View;

namespace DCCPanelController.ViewModel;

public partial class TurnoutsViewModel : BaseViewModel {

    private const string LabelID = "ID";
    private const string LabelName = "Turnout";
    private const string LabelState = "State";
    
    private INavigationService NavigationService { get; }
    private ConnectionService ConnectionService { get; }
    private bool _isAscending = false;
    private string _sortColumn = "";

    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;
    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;

    public TurnoutsViewModel(TurnoutsService? turnoutStateService, ConnectionService connectionService, INavigationService navigationService) {
        ConnectionService = connectionService;
        NavigationService = navigationService;
        Turnouts = turnoutStateService?.Turnouts ?? [];
        SetLabels();
    }

    [RelayCommand]
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
        ColumnLabelID = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    public async Task EditTurnoutAsync(Turnout turnout) {
        await NavigationService.NavigateToEditTurnoutAsync(turnout);
    }

    [RelayCommand]
    public async Task AddTurnoutAsync() {
        var turnout = new Turnout {
            Id = "NT0000",
            Name = "Example Turnout",
            State = TurnoutStateEnum.Closed
        };
        if (await NavigationService.NavigateToEditTurnoutAsync(turnout) is { } result) {
            Turnouts.Add(result);
        }
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