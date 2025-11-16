using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Turnout = DCCPanelController.Models.DataModel.Accessories.Turnout;

namespace DCCPanelController.View;

public partial class TurnoutsEditViewModel : BaseViewModel {
    private readonly             ILogger<TurnoutsEditViewModel> _logger;
    [ObservableProperty] private string                         _title;
    [ObservableProperty] private Turnout                        _turnout;

    public TurnoutsEditViewModel(ILogger<TurnoutsEditViewModel> logger, Turnout turnout, ConnectionService connectionService) {
        _logger = logger;
        Turnout = turnout;
        ConnectionService = connectionService;
        Title = Turnout?.Name ?? "Turnout Properties";
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