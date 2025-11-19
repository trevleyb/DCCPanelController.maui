using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Block = DCCPanelController.Models.DataModel.Accessories.Block;

namespace DCCPanelController.View;

public partial class BlocksEditViewModel : BaseViewModel {
    private readonly             ILogger<BlocksEditViewModel> _logger;
    [ObservableProperty] private string                       _title;
    [ObservableProperty] private Block                        _block;
    [ObservableProperty] private Sensor?                      _sensor;

    public ObservableCollection<Sensor> Sensors { get; init; }
    
    public BlocksEditViewModel(ILogger<BlocksEditViewModel> logger, Block block, ObservableCollection<Sensor> sensors, ConnectionService connectionService) {
        _logger = logger;
        Block = block;
        Sensors = sensors;
        Sensor = sensors.FirstOrDefault(x => x?.Id == block?.Sensor);
        ConnectionService = connectionService;
        Title = Block?.Name ?? "Block Properties";
    }

    private ConnectionService ConnectionService { get; }

    public static Task ApplyChangesAsync() => Task.CompletedTask;

    public ContentView CreatePropertiesView() {
        var propPage = new BlocksEditView(_logger, this);
        return propPage;
    }

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
        //if (ConnectionService.Client is { } client) await client.SendTurnoutCmdAsync(Turnout, Turnout.State == TurnoutStateEnum.Thrown);
        //OnPropertyChanged(nameof(Turnout.State));
        //OnPropertyChanged(nameof(Turnout));
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
        //OnPropertyChanged(nameof(Turnout.Default));
        //OnPropertyChanged(nameof(Turnout));
    }
}