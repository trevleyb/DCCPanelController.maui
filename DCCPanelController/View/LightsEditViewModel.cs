using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class LightsEditViewModel : BaseViewModel {
    private readonly             ILogger<LightsEditViewModel> _logger;
    [ObservableProperty] private bool                         _isManualControlled;
    [ObservableProperty] private bool                         _isServerControlled;
    [ObservableProperty] private string                       _title;
    [ObservableProperty] private Light                        _light;

    public LightsEditViewModel(ILogger<LightsEditViewModel> logger, Light light, ConnectionService connectionService) {
        _logger = logger;
        Light = light;
        ConnectionService = connectionService;
        Title = Light?.Name ?? "Light Properties";
        IsServerControlled = Light?.IsEditable == false;
        IsManualControlled = Light?.IsEditable == true;
    }

    private ConnectionService ConnectionService { get; }

    public static Task ApplyChangesAsync() => Task.CompletedTask;

    public ContentView CreatePropertiesView() {
        var propPage = new LightsEditView(_logger, this);
        return propPage;
    }

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
    //    if (ConnectionService.Client is { } client) await client.SendTurnoutCmdAsync(Turnout, Turnout.State == TurnoutStateEnum.Thrown);
    //    OnPropertyChanged(nameof(Turnout.State));
    //    OnPropertyChanged(nameof(Turnout));
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
    //    OnPropertyChanged(nameof(Turnout.Default));
    //    OnPropertyChanged(nameof(Turnout));
    }
}