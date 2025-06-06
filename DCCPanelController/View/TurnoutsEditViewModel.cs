using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Properties;
using Turnout = DCCPanelController.Models.DataModel.Turnout;

namespace DCCPanelController.View;

public partial class TurnoutsEditViewModel : Base.BaseViewModel, IPropertiesViewModel {
    [ObservableProperty] private string _title;
    [ObservableProperty] private Turnout _turnout;

    public TurnoutsEditViewModel(Turnout turnout, ConnectionService connectionService) {
        Turnout = turnout;
        ConnectionService = connectionService;
        Title = Turnout?.Name ?? "Turnout Properties";
    }

    private ConnectionService ConnectionService { get; }
    private IDccClient? Client { get; set; }

    public Task ApplyChangesAsync() {
        return Task.CompletedTask;
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {
        var propPage = new TurnoutsEditView(this);
        return propPage;
    }

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync() {
        if (!string.IsNullOrEmpty(Turnout.DccAddress)) {
            ConnectionService?.SendTurnoutCmdAsync(Turnout.DccAddress, Turnout.State == TurnoutStateEnum.Thrown);
            OnPropertyChanged(nameof(Turnout.State));
            OnPropertyChanged(nameof(Turnout));
        }
    }

    [RelayCommand]
    private async Task ToggleTurnoutDefaultStateAsync() {
        // Turnout.Default = Turnout.Default switch {
        //     TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
        //     TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
        //     _                       => TurnoutStateEnum.Closed
        // };
        OnPropertyChanged(nameof(Turnout.Default));
        OnPropertyChanged(nameof(Turnout));
    }
}