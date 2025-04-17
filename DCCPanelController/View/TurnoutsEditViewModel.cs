using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using Turnout = DCCPanelController.Models.DataModel.Turnout;

namespace DCCPanelController.View;

public partial class TurnoutsEditViewModel : BaseViewModel {
    [ObservableProperty] private TurnoutStateEnum _currentState;
    [ObservableProperty] private string? _dccAddress;
    [ObservableProperty] private TurnoutStateEnum _defaultState;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private string _systemName;

    [ObservableProperty] private Turnout? _turnout;
    [ObservableProperty] private string _userName;

    public TurnoutsEditViewModel(Turnout turnout, ConnectionService connectionService) {
        Turnout = turnout;
        ConnectionService = connectionService;
        SystemName = Turnout.Id ?? "NT001";
        UserName = Turnout.Name ?? "New Turnout";
        DccAddress = Turnout.DccAddress;
        CurrentState = Turnout.State;
        DefaultState = Turnout.Default;
        IsEditable = Turnout.IsEditable;
    }

    private ConnectionService ConnectionService { get; }
    private IDccClient? Client { get; set; }

    public event Action<Turnout?>? OnSaveCompleted;
    public event EventHandler? CloseRequested;

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
        if (Turnout == null) return;
        if (!string.IsNullOrEmpty(Turnout.Id)) {
            Client = await ConnectionService.Connect();
            Client?.SendTurnoutCmd(Turnout.Id, CurrentState == TurnoutStateEnum.Thrown);
        }
        OnPropertyChanged(nameof(CurrentState));
        OnPropertyChanged(nameof(Turnout));
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
        DefaultState = DefaultState switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };
        OnPropertyChanged(nameof(DefaultState));
        OnPropertyChanged(nameof(Turnout));
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