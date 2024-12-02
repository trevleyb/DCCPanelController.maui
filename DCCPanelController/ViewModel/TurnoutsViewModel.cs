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

    private bool _isAscending = false;
    private string _sortColumn = "";
    
    private INavigationService NavigationService { get; init; }
    private ConnectionService ConnectionService { get; init; }
    private TurnoutsService TurnoutService { get; init; }

    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;
    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;

    public TurnoutsViewModel(TurnoutsService turnoutService, ConnectionService connectionService, INavigationService navigationService) {
        TurnoutService    = turnoutService;
        ConnectionService = connectionService;
        NavigationService = navigationService;
        
        Turnouts = TurnoutService.Turnouts ?? [];
        SetLabels();
    }
    
    private void SetLabels() {
        ColumnLabelID    = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName  = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
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
            Id = TurnoutAnalyzer.GetUniqueID(Turnouts.ToList()),
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