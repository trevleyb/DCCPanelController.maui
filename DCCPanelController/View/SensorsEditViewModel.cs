using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Turnout = DCCPanelController.Models.DataModel.Turnout;

namespace DCCPanelController.View;

public partial class SensorsEditViewModel : BaseViewModel {
    private readonly             ILogger<SensorsEditViewModel> _logger;
    [ObservableProperty] private bool                          _isManualControlled;
    [ObservableProperty] private bool                          _isServerControlled;
    [ObservableProperty] private string                        _title;
    [ObservableProperty] private Sensor                        _sensor;

    public SensorsEditViewModel(ILogger<SensorsEditViewModel> logger, Sensor sensor, ConnectionService connectionService) {
        _logger = logger;
        Sensor = sensor;
        ConnectionService = connectionService;
        Title = Sensor?.Name ?? "Sensor Properties";
        IsServerControlled = Sensor?.IsEditable == false;
        IsManualControlled = Sensor?.IsEditable == true;
    }

    private ConnectionService ConnectionService { get; }

    public static Task ApplyChangesAsync() => Task.CompletedTask;

    public ContentView CreatePropertiesView() {
        var propPage = new SensorsEditView((ILogger<SensorsEditView>)_logger, this);
        return propPage;
    }

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
        ///if (ConnectionService.Client is { } client) await client.SendTurnoutCmdAsync(Turnout, Turnout.State == TurnoutStateEnum.Thrown);
        //OnPropertyChanged(nameof(Turnout.State));
        //OnPropertyChanged(nameof(Turnout));
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
        //OnPropertyChanged(nameof(Turnout.Default));
        //OnPropertyChanged(nameof(Turnout));
    }
}