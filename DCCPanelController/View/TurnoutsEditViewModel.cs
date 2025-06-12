using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Properties;
using Turnout = DCCPanelController.Models.DataModel.Turnout;

namespace DCCPanelController.View;

public partial class TurnoutsEditViewModel : Base.BaseViewModel, IPropertiesViewModel {
    [ObservableProperty] private string _title;
    [ObservableProperty] private Turnout _turnout;
    [ObservableProperty] private bool _isServerControlled;
    [ObservableProperty] private bool _isManualControlled;

    public TurnoutsEditViewModel(Turnout turnout, ConnectionService connectionService) {
        Turnout = turnout;
        ConnectionService = connectionService;
        Title = Turnout?.Name ?? "Turnout Properties";
        IsServerControlled = Turnout?.IsEditable == false;
        IsManualControlled = Turnout?.IsEditable == true ;
    }

    private ConnectionService ConnectionService { get; }

    public Task ApplyChangesAsync() {
        return Task.CompletedTask;
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {
        var propPage = new TurnoutsEditView(this);
        return propPage;
    }

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
        if (ConnectionService.Client is { } client) await client.SendTurnoutCmdAsync(Turnout, Turnout.State == TurnoutStateEnum.Thrown);
        OnPropertyChanged(nameof(Turnout.State));
        OnPropertyChanged(nameof(Turnout));
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
        OnPropertyChanged(nameof(Turnout.Default));
        OnPropertyChanged(nameof(Turnout));
    }
}