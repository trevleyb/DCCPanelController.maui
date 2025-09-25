using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class BlocksEditViewModel : BaseViewModel {
    private readonly             ILogger<BlocksEditViewModel> _logger;
    [ObservableProperty] private bool                         _isManualControlled;
    [ObservableProperty] private bool                         _isServerControlled;
    [ObservableProperty] private string                       _title;
    [ObservableProperty] private Block                        _block;

    public BlocksEditViewModel(ILogger<BlocksEditViewModel> logger, Block block, ConnectionService connectionService) {
        _logger = logger;
        Block = block;
        ConnectionService = connectionService;
        Title = Block?.Name ?? "Block Properties";
        IsServerControlled = Block?.IsEditable == false;
        IsManualControlled = Block?.IsEditable == true;
    }

    private ConnectionService ConnectionService { get; }

    public static Task ApplyChangesAsync() => Task.CompletedTask;

    public ContentView CreatePropertiesView() {
        var propPage = new BlocksEditView((ILogger<BlocksEditView>)_logger, this);
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