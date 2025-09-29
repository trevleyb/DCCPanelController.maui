using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Turnout = DCCPanelController.Models.DataModel.Turnout;

namespace DCCPanelController.View;

public partial class TurnoutsEditViewModel : BaseViewModel {
    private readonly             ILogger<TurnoutsEditViewModel> _logger;
    [ObservableProperty] private bool                           _isManualControlled;
    [ObservableProperty] private bool                           _isServerControlled;
    [ObservableProperty] private string                         _title;
    [ObservableProperty] private Turnout                        _turnout;

    public TurnoutsEditViewModel(ILogger<TurnoutsEditViewModel> logger, Turnout turnout, ConnectionService connectionService) {
        _logger = logger;
        Turnout = turnout;
        ConnectionService = connectionService;
        Title = Turnout?.Name ?? "Turnout Properties";
        IsServerControlled = Turnout?.IsEditable == false;
        IsManualControlled = Turnout?.IsEditable == true;
    }

    private ConnectionService ConnectionService { get; }

    public static Task ApplyChangesAsync() => Task.CompletedTask;

    public ContentView CreatePropertiesView() {
        var propPage = new TurnoutsEditView(_logger, this);
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