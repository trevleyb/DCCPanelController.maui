using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class TurnoutsEditViewModel : BaseViewModel {
    [ObservableProperty] private TurnoutStateEnum _currentState;
    [ObservableProperty] private int? _dccAddress;
    [ObservableProperty] private TurnoutStateEnum _defaultState;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private string _systemName;

    [ObservableProperty] private Turnout? _turnout;
    [ObservableProperty] private string _userName;

    public TurnoutsEditViewModel(Turnout turnout) {
        Turnout = turnout;
        SystemName = Turnout.Id ?? "NT001";
        UserName = Turnout.Name ?? "New Turnout";
        DccAddress = Turnout.DccAddress;
        CurrentState = Turnout.State;
        DefaultState = Turnout.Default;
        IsEditable = Turnout.IsEditable;
        ConnectionService = MauiProgram.ServiceHelper.GetService<ConnectionService>();
    }

    private ConnectionService ConnectionService { get; }

    public event Action<Turnout?>? OnSaveCompleted;
    public event EventHandler? CloseRequested;

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
        if (Turnout == null) return;
        if (!string.IsNullOrEmpty(Turnout.Id)) ConnectionService?.SendTurnoutStateChangeCommand(Turnout.Id, CurrentState);
        OnPropertyChanged(nameof(CurrentState));
        OnPropertyChanged(nameof(Turnout));
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
        // DefaultState = DefaultState switch {
        //      TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
        //      TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
        //      _                       => TurnoutStateEnum.Closed
        // };
        // OnPropertyChanged(nameof(DefaultState));
        // OnPropertyChanged(nameof(Turnout));
    }

    [RelayCommand]
    private async Task SaveAsync() {
        if (Turnout is not null) {
            Turnout.Id = SystemName;
            Turnout.Name = UserName;
            Turnout.Default = DefaultState;
            Turnout.DccAddress = DccAddress;
            OnSaveCompleted?.Invoke(Turnout);
            CloseRequested?.Invoke(Turnout, EventArgs.Empty);
        }

        CloseRequested?.Invoke(null, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task CancelAsync() {
        OnSaveCompleted?.Invoke(null);
        CloseRequested?.Invoke(null, EventArgs.Empty);
    }
}