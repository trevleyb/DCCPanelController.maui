using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View.Base;

public partial class ConnectionViewModel : BaseViewModel {

    [ObservableProperty] private bool _isConnected = false;
    
    protected ConnectionViewModel(Profile profile, ConnectionService connectionService) {
        Profile = profile;
        ConnectionService = connectionService;
        ConnectionService.ConnectionChanged += (sender, args) => {
            IsConnected = args.IsConnected;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectionIcon));
            OnPropertyChanged(nameof(IsConnectionAvailable));
        };
    }

    public Profile Profile { get; set; }
    public ConnectionService ConnectionService { get; }
    public bool IsConnectionAvailable => Profile?.Settings?.ClientSettings?.Type != DccClientType.Unknown; 
    public string ConnectionIcon => IsConnected ? "server_on.png" : "server_off.png";

    [RelayCommand]
    protected async Task ToggleConnectionAsync() {
        var result = await ConnectionService.ToggleConnectionAsync();
        if (result.IsFailure) {
            var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result.Message) ? "." : $" due to {result.Message}")}";            
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
        }
    }
}