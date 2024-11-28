using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.ViewModel;

public partial class TurnoutsEditViewModel : BaseViewModel {

    public event EventHandler? CloseRequested;
    private readonly Turnout _turnout;
    
    public TurnoutsEditViewModel(Turnout turnout) {
        _turnout = turnout;
        SystemName = turnout.Id ?? "ID0000";
        UserName = turnout.Name ?? "Unknown Turnout";
        CurrentState = _turnout.State;
        DefaultState = _turnout.Default;
    }

    [ObservableProperty] private string _systemName;
    [ObservableProperty] private string _userName;
    [ObservableProperty] private TurnoutStateEnum _defaultState;
    [ObservableProperty] private TurnoutStateEnum _currentState;
    
    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
        CurrentState = CurrentState switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
        DefaultState = DefaultState switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };
    }

    [RelayCommand]
    private async Task SaveAsync() {
        _turnout.Id = SystemName;
        _turnout.Name = UserName;
        _turnout.Default = DefaultState;
        CloseRequested?.Invoke(_turnout, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task CancelAsync() {
        CloseRequested?.Invoke(null, EventArgs.Empty);
    }
   
}