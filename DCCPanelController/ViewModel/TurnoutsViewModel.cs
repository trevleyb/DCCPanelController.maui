using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Services;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace DCCPanelController.ViewModel;

public partial class TurnoutsViewModel : BaseViewModel {

    public ObservableCollection<Turnout> Turnouts { get; set; }
    private readonly TurnoutsService? _turnoutStateService;
    private bool _isAscending = false;
    private string _sortColumn = "";
    
    public TurnoutsViewModel(TurnoutsService? turnoutStateService) {
        _turnoutStateService = turnoutStateService;
        Turnouts = _turnoutStateService?.Turnouts ?? new ObservableCollection<Turnout>();
        SetLabels();
    }

    [ObservableProperty] private string columnLabelID       = "ID";
    [ObservableProperty] private string columnLabelName     = "Turnout Name";
    [ObservableProperty] private string columnLabelState    = "State";
    
    [RelayCommand]
    public async Task SortByColumn(string columnName) {
        List<Turnout> sortedTurnout;
        if (!_isAscending) {
            sortedTurnout = columnName.ToLower() switch {
                "name"  => Turnouts.OrderBy(x => x.Name).ToList(),
                "id"    => Turnouts.OrderBy(x => x.Id).ToList(),
                "state" => Turnouts.OrderBy(x => x.State).ToList(),
                _       => Turnouts.ToList(),
            };
        } else {
            sortedTurnout = columnName.ToLower() switch {
                "name"  => Turnouts.OrderByDescending(x => x.Name).ToList(),
                "id"    => Turnouts.OrderByDescending(x => x.Id).ToList(),
                "state" => Turnouts.OrderByDescending(x => x.State).ToList(),
                _       => Turnouts.ToList(),
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
        ColumnLabelName = "Turnout Name" + (_sortColumn.Equals("Name") ? _isAscending.GetSortDirection() : "");
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
        
        var connectionSerice = App.ServiceProvider?.GetService<ConnectionService>();
        if (connectionSerice != null && !string.IsNullOrEmpty(turnout.Id)) {
            connectionSerice.SendTurnoutStateChangeCommand(turnout.Id, turnout.State);
        }
    }
}
